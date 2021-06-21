using System;
using System.IO;
using System.Linq;
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

        public static byte[] Encrypt(string plainText)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(EncryptionKey), Encoding.UTF8.GetBytes(InitVector));
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return encrypted;
        }
        public static string Decrypt(byte[] cipherText)
        {
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(EncryptionKey), Encoding.UTF8.GetBytes(InitVector));
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        ///// <summary>
        /////     Encrypts the data with AES-256, CBC with a random init vector (IV)
        ///// </summary>
        ///// <param name="data">The data to encrypt</param>
        ///// <returns>The encrypted data, encoded to base64</returns>
        //public static string Encrypt(string data)
        //{
        //    using var rijAlg = new RijndaelManaged();

        //    rijAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        //    rijAlg.IV = Encoding.UTF8.GetBytes(InitVector);

        //    var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

        //    using var msEncrypt = new MemoryStream();
        //    using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        //    using var swEncrypt = new StreamWriter(csEncrypt);

        //    swEncrypt.Write(data);

        //    var encrypted = msEncrypt.ToArray();

        //    return Convert.ToBase64String(encrypted);
        //}

        ///// <summary>
        /////     Decrypts the data from base64 to plain-text
        ///// </summary>
        ///// <param name="data">The data to decrypt</param>
        ///// <returns>The decrypted data</returns>
        //public static string Decrypt(string data)
        //{
        //    var cipherText = Convert.FromBase64String(data.Replace(' ', '+'));

        //    using var rijAlg = new RijndaelManaged();
        //    rijAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        //    rijAlg.IV = Encoding.UTF8.GetBytes(InitVector);

        //    var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

        //    using var msDecrypt = new MemoryStream(cipherText);
        //    using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        //    using var srDecrypt = new StreamReader(csDecrypt);

        //    var plaintext = srDecrypt.ReadToEnd();

        //    return plaintext;
        //}
    }
}