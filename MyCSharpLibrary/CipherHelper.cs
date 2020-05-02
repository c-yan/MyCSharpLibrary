using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyCSharpLibrary
{
    public static class CipherHelper
    {
        static readonly int PBKDF2IterationCount = 10000;

        static byte[] GenerateSalt(int saltLength)
        {
            var result = new byte[saltLength];
            using (var csp = new RNGCryptoServiceProvider())
            {
                csp.GetBytes(result);
                return result;
            }
        }

        static byte[] GenerateKIV(byte[] salt, string password, HashAlgorithmName hashAlgorithm, int iterationCount, int size)
        {
#if NETSTANDARD2_1 || NET48
            return new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, iterationCount, hashAlgorithm)
                .GetBytes(size);
#else
            return new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, iterationCount)
                .GetBytes(size);
#endif
        }

        public static string Encrypt(byte[] plainBytes, string password)
        {
            var salt = GenerateSalt(8);
            var kiv = GenerateKIV(salt, password, HashAlgorithmName.SHA256, PBKDF2IterationCount, 48).ToList();
            using (var csp = new AesManaged())
            {
                csp.KeySize = 256;
                csp.BlockSize = 128;
                csp.Mode = CipherMode.CBC;
                csp.Padding = PaddingMode.PKCS7;
                csp.Key = kiv.GetRange(0, 32).ToArray();
                csp.IV = kiv.GetRange(32, 16).ToArray();
                using (var encryptor = csp.CreateEncryptor())
                using (var mstream = new MemoryStream())
                {
                    mstream.Write(Encoding.UTF8.GetBytes("Salted__"), 0, 8);
                    mstream.Write(salt, 0, 8);
                    using (var cstream = new CryptoStream(mstream, encryptor, CryptoStreamMode.Write))
                    {
                        cstream.Write(plainBytes, 0, plainBytes.Length);
                    }
                    return Convert.ToBase64String(mstream.ToArray());
                }
            }
        }

        public static byte[] Decrypt(string encryptedString, string password)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedString);
            var salt = encryptedBytes.Skip(8).Take(8).ToArray();
            var kiv = GenerateKIV(salt, password, HashAlgorithmName.SHA256, PBKDF2IterationCount, 48).ToList();
            using (var csp = new AesManaged())
            {
                csp.KeySize = 256;
                csp.BlockSize = 128;
                csp.Mode = CipherMode.CBC;
                csp.Padding = PaddingMode.PKCS7;
                csp.Key = kiv.GetRange(0, 32).ToArray();
                csp.IV = kiv.GetRange(32, 16).ToArray();
                using (var decryptor = csp.CreateDecryptor())
                using (var mstream1 = new MemoryStream(encryptedBytes.Skip(16).ToArray()))
                using (var cstream = new CryptoStream(mstream1, decryptor, CryptoStreamMode.Read))
                using (var mstream2 = new MemoryStream())
                {
                    cstream.CopyTo(mstream2);
                    return mstream2.ToArray();
                }
            }
        }
    }
}
