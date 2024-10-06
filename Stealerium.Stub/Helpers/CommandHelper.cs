using System;
using System.Diagnostics;

namespace Stealerium.Stub.Helpers
{
    internal sealed class CommandHelper
    {
        /// <summary>
        /// Runs a command using cmd.exe and returns the output.
        /// </summary>
        /// <param name="cmd">The command to run.</param>
        /// <param name="wait">If true, waits for the command to complete before returning the output.</param>
        /// <returns>The output from the command execution.</returns>
        public static string Run(string cmd, bool wait = true)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        Arguments = $"/C {cmd}", // /C runs the command and then exits
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };

                    process.Start();

                    // Capture standard output and error
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (wait)
                    {
                        process.WaitForExit();
                    }

                    // Return error if any, otherwise return the output
                    return string.IsNullOrEmpty(error) ? output : $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                // Log and return the exception message
                Logging.Log($"CommandHelper: Exception occurred while running command '{cmd}': {ex.Message}");
                return $"Exception: {ex.Message}";
            }
        }
    }
}
