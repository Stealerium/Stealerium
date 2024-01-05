using System.Security.Cryptography;
using System.Text;

namespace Builder.Modules
{
    internal sealed class Crypt
    {
        private static readonly byte[] SaltBytes = {
            102, 51, 111, 51, 75, 45, 49, 49, 61, 71,
            45, 78, 55, 86, 74, 116, 111, 122, 79, 87,
            82, 114, 61, 40, 116, 78, 90, 66, 102, 75,
            43, 98, 83, 55, 70, 121
        };

        private static readonly byte[] CryptKey = {
            59, 38, 75, 70, 33, 77, 33, 104, 56, 94,
            105, 84, 58, 60, 41, 97, 63, 126, 109, 88,
            101, 78, 42, 126, 111, 63, 103, 78, 91, 118,
            64, 114, 81, 61, 66
        };

        public static string EncryptConfig(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(value);
            byte[] encryptedBytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(CryptKey, SaltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return "ENCRYPTED:" + Convert.ToBase64String(encryptedBytes);
        }
    }
}
