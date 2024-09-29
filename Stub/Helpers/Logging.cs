using System;
using System.IO;
using System.Text;

namespace Stealerium.Helpers
{
    internal sealed class Logging
    {
        private static readonly string Logfile = Path.Combine(Path.GetTempPath(), "Stealerium-Latest.log");
        private static readonly object _lock = new object();
        private static readonly StringBuilder logBuffer = new StringBuilder();

        // Formats log entry with timestamp for readability
        private static string FormatLogEntry(string text)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"[{timestamp}] {text}{Environment.NewLine}";
        }

        // Logs text to the logfile and buffers it to StringBuilder
        public static bool Log(string text, bool ret = true)
        {
            var formattedText = FormatLogEntry(text);

            // Only log if DebugMode is enabled
            if (Config.DebugMode == "1")
            {
                lock (_lock)
                {
                    logBuffer.Append(formattedText); // Buffer log in memory
                    File.AppendAllText(Logfile, formattedText); // Write to log file
                }
            }
            return ret;
        }

        // Saves the current log file to a new path
        public static void Save(string sSavePath)
        {
            if (Config.DebugMode != "1" || !File.Exists(Logfile)) return;
            try
            {
                File.Copy(Logfile, sSavePath, true);
            }
            catch (Exception ex)
            {
                //
            }
        }

        // Returns the buffered log content
        public static string GetBufferedLogs()
        {
            lock (_lock)
            {
                return logBuffer.ToString();
            }
        }
    }
}
