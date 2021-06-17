using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LightVPN.Client.Cryptography
{
    public static class Aes
    {
        private const string EncryptionKey =
            "TxEpeRwPtwV4hqrPTZ2y9vEyq6jsypje";

        private static AesManaged CreateAesManaged()
        {
            var aesManaged = new AesManaged();
            aesManaged.GenerateIV();
            aesManaged.KeySize = 256;
            aesManaged.Mode = CipherMode.CBC;
            aesManaged.Padding = PaddingMode.PKCS7;

            aesManaged.Key = Encoding.UTF8.GetBytes(EncryptionKey);

            return aesManaged;
        }

        public static string Encrypt(string data)
        {
            using var aesManaged = CreateAesManaged();

            var cryptor = aesManaged.CreateEncryptor();

            var encryptedBytes = Array.Empty<byte>();

            using var stream = new MemoryStream();
            using var cryptoStream = new CryptoStream(stream, cryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cryptoStream);

            writer.Write(data);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string data)
        {
            using var aesManaged = CreateAesManaged();

            var cryptor = aesManaged.CreateDecryptor();

            var decodedBytes = Convert.FromBase64String(data.Replace(' ', '+'));

            using var stream = new MemoryStream(decodedBytes);
            using var cryptoStream = new CryptoStream(stream, cryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }
    }
}