using System;
using System.IO;

namespace Stealerium.Helpers
{
    internal sealed class Logging
    {
        private static readonly string Logfile = Path.Combine(Path.GetTempPath(), "Stealerium-Latest.log");

        public static bool Log(string text, bool ret = true)
        {
            var newline = "\n";
            if (text.Length > 40 && text.Contains("\n"))
                newline += "\n\n";
            Console.Write(text + newline);
            if (Config.DebugMode == "1")
                File.AppendAllText(Logfile, text + newline);
            return ret;
        }

        public static void Save(string sSavePath)
        {
            if (Config.DebugMode != "1" || !File.Exists(Logfile)) return;
            try
            {
                File.Copy(Logfile, sSavePath);
            }
            catch
            {
                // ignored
            }
        }
    }
}