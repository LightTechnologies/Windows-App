using System;
using System.IO;
using System.Text.Json;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Cryptography;
using LightVPN.Client.Windows.Common;

namespace LightVPN.Client.Auth.Utils
{
    /// <summary>
    ///     Handles writing the authentication data to the auth data file
    /// </summary>
    internal static class AuthDataManager
    {
        static AuthDataManager()
        {
            Verify();
        }

        /// <summary>
        ///     Verifies the existence of the authentication data file
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the LightVPN folder couldn't be created due to some exception</exception>
        private static void Verify()
        {
            if (!Directory.Exists(Path.GetDirectoryName(Globals.AuthDataPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(Globals.AuthDataPath) ??
                                          throw new InvalidOperationException());

            if (!File.Exists(Globals.AuthDataPath)) Write(new AuthResponse());
        }

        /// <summary>
        ///     Encrypts and writes a AuthResponse to the authentication data file
        /// </summary>
        /// <param name="authResponse">The response to write</param>
        public static void Write(AuthResponse authResponse)
        {
            var json = JsonSerializer.Serialize(authResponse);

            var encryptedData = Aes.Encrypt(json);

            File.WriteAllText(Globals.AuthDataPath, encryptedData);
        }

        /// <summary>
        ///     Decrypts and returns a AuthResponse from the authentication data file
        /// </summary>
        /// <returns></returns>
        public static AuthResponse Read()
        {
            Verify();

            var encryptedData = File.ReadAllText(Globals.AuthDataPath);

            var decryptedContent = Aes.Decrypt(encryptedData);

            return JsonSerializer.Deserialize<AuthResponse>(decryptedContent);
        }
    }
}