/* --------------------------------------------
 * 
 * AES-256 Encryption / decryption - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: Encryption.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Settings.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LightVPN.Settings
{
    public class Encryption : IEncryption
    {
        /// <summary>
        /// Internal encryption key used for this class
        /// </summary>
        internal readonly string EncryptionKey = "h46V2usZ5y2sMMDQRjcFQHXFGrLMrQSkEGGzeLBevCJ2MeGQRUE2k3pMUP4jTQrhcgEe5VgpwsThdHmJM3XbnLdvrk";
        /// <summary>
        /// Decrypts the specified base64 encoded string
        /// </summary>
        /// <param name="cipherText">Input string, encoded with base64</param>
        /// <returns>Input string decrypted into plaintext, if decryption was successful</returns>
        public string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
                Rfc2898DeriveBytes pdb = new(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            return cipherText;
        }
        /// <summary>
        /// Encrypts the input plaintext with the AES encryption standard, on the CBC cipher
        /// </summary>
        /// <param name="clearText">Input string in plaintext to be encrypted</param>
        /// <returns>Encrypted text encoded in base64</returns>
        public string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using var encryptor = Aes.Create();
                encryptor.Mode = CipherMode.CBC;
                Rfc2898DeriveBytes pdb = new(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            return clearText;
        }

    }
}
