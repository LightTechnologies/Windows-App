/* --------------------------------------------
 * 
 * AES-256 Encryption / decryption - Interface
 * Copyright (C) Light Technologies LLC
 * 
 * File: IEncryption.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
namespace LightVPN.Settings.Interfaces
{
    public interface IEncryption
    {
        string Decrypt(string cipherText);
        string Encrypt(string clearText);
    }
}