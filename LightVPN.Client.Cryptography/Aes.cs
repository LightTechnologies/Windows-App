using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LightVPN.Client.Cryptography
{
    /// <summary>
    ///     Handles encryption of the authentication data file
    /// </summary>
    public static class Aes
    {
        /// <summary>
        ///     Encryption key for the authentication data file
        /// </summary>
        private const string EncryptionKey =
            "TxEpeRwPtwV4hqrPTZ2y9vEyq6jsypje";

        /// <summary>
        ///     Creates a instance of the AesManaged class
        /// </summary>
        /// <returns>The new configured AesManaged class</returns>
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

        /// <summary>
        ///     Encrypts the data with AES-256, CBC with a random init vector (IV)
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <returns>The encrypted data, encoded to base64</returns>
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

        /// <summary>
        ///     Decrypts the data from base64 to plain-text
        /// </summary>
        /// <param name="data">The data to decrypt</param>
        /// <returns>The decrypted data</returns>
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