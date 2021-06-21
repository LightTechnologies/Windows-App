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
        ///     IV for encryption (this is stupid i know)
        /// </summary>
        private const string InitVector = "?^Y)2Q`cr#pZAxZ*";

        ///// <summary>
        /////     Encrypts the data with AES-256, CBC with a random init vector (IV)
        ///// </summary>
        ///// <param name="data">The data to encrypt</param>
        ///// <returns>The encrypted data in bytes</returns>
        public static byte[] Encrypt(string plainText)
        {
            using var aes = new AesManaged();

            var encryptor =
                aes.CreateEncryptor(Encoding.UTF8.GetBytes(EncryptionKey), Encoding.UTF8.GetBytes(InitVector));

            using var ms = new MemoryStream();

            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var encrypted = ms.ToArray();

            return encrypted;
        }

        ///// <summary>
        /////     Decrypts the data from bytes to plain-text
        ///// </summary>
        ///// <param name="data">The data to decrypt</param>
        ///// <returns>The decrypted data</returns>
        public static string Decrypt(byte[] cipherText)
        {
            using var aes = new AesManaged();

            var decryptor =
                aes.CreateDecryptor(Encoding.UTF8.GetBytes(EncryptionKey), Encoding.UTF8.GetBytes(InitVector));

            using var ms = new MemoryStream(cipherText);

            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            using var reader = new StreamReader(cs);
            var plaintext = reader.ReadToEnd();

            return plaintext;
        }
    }
}