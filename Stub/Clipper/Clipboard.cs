using System.Threading;

namespace Stealerium.Clipper
{
    /// <summary>
    /// The Clipboard class provides methods to interact with the system clipboard
    /// by getting and setting text.
    /// </summary>
    internal sealed class Clipboard
    {
        /// <summary>
        /// Gets the text content from the system clipboard.
        /// </summary>
        /// <returns>A string containing the clipboard text, or an empty string if no text is available.</returns>
        public static string GetText()
        {
            var returnValue = string.Empty;

            try
            {
                // Create a new thread to access the clipboard, ensuring it runs in STA mode
                var staThread = new Thread(() => { returnValue = System.Windows.Forms.Clipboard.GetText(); });
                staThread.SetApartmentState(ApartmentState.STA); // Set the thread's apartment state to STA (required for clipboard access)
                staThread.Start();
                staThread.Join(); // Wait for the thread to complete
            }
            catch
            {
                // Silently catch and ignore exceptions
            }

            return returnValue;
        }

        /// <summary>
        /// Sets the specified text to the system clipboard.
        /// </summary>
        /// <param name="text">The text to set on the clipboard.</param>
        public static void SetText(string text)
        {
            // Create a new thread to set the clipboard text, ensuring it runs in STA mode
            var staThread = new Thread(() =>
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(text);
                }
                catch
                {
                    // Silently catch and ignore exceptions
                }
            });
            staThread.SetApartmentState(ApartmentState.STA); // Set the thread's apartment state to STA (required for clipboard access)
            staThread.Start();
            staThread.Join(); // Wait for the thread to complete
        }
    }
}
