using System;
using System.IO;
using System.Text;

namespace Stealerium.Helpers
{
    /// <summary>
    /// The Logging class handles logging messages to a file and buffering logs in memory.
    /// </summary>
    internal sealed class Logging
    {
        // Path to the log file stored in the temporary directory
        private static readonly string Logfile = Path.Combine(Path.GetTempPath(), "Stealerium-Latest.log");

        // Object used for locking to ensure thread safety
        private static readonly object _lock = new object();

        // StringBuilder for buffering log messages in memory
        private static readonly StringBuilder logBuffer = new StringBuilder();

        /// <summary>
        /// Formats a log entry with a timestamp for better readability.
        /// </summary>
        /// <param name="text">The log message to be formatted.</param>
        /// <returns>A string with the formatted log entry including a timestamp.</returns>
        private static string FormatLogEntry(string text)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"[{timestamp}] {text}{Environment.NewLine}";
        }

        /// <summary>
        /// Logs a message to both the log file and in-memory buffer.
        /// The log entry is only created if DebugMode is enabled in the configuration.
        /// </summary>
        /// <param name="text">The message to log.</param>
        /// <param name="ret">The return value, default is true.</param>
        /// <returns>Returns the value of ret (default: true).</returns>
        public static bool Log(string text, bool ret = true)
        {
            var formattedText = FormatLogEntry(text);

            // Log only if DebugMode is enabled
            if (Config.DebugMode == "1")
            {
                lock (_lock) // Ensure thread-safe access to log buffer and file
                {
                    logBuffer.Append(formattedText); // Buffer the log in memory
                    File.AppendAllText(Logfile, formattedText); // Append log to file
                }
            }

            return ret; // Return the result (default is true)
        }

        /// <summary>
        /// Saves the current log file to a new path.
        /// </summary>
        /// <param name="sSavePath">The path where the log file will be saved.</param>
        public static void Save(string sSavePath)
        {
            // Only save the log if DebugMode is enabled and the log file exists
            if (Config.DebugMode != "1" || !File.Exists(Logfile)) return;

            try
            {
                File.Copy(Logfile, sSavePath, overwrite: true); // Copy log file to the new path
            }
            catch (Exception ex)
            {
                // Handle potential exceptions (could add logging of the error here)
            }
        }

        /// <summary>
        /// Retrieves the current content of the in-memory log buffer.
        /// </summary>
        /// <returns>A string containing the buffered logs.</returns>
        public static string GetBufferedLogs()
        {
            lock (_lock) // Ensure thread-safe access to the log buffer
            {
                return logBuffer.ToString(); // Return the content of the buffer
            }
        }
    }
}
