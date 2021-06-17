using System;
using System.IO;
using System.Text.Json;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Cryptography;
using LightVPN.Client.Windows.Common;

namespace LightVPN.Client.Auth.Utils
{
    internal static class AuthDataManager
    {
        static AuthDataManager()
        {
            Verify();
        }

        private static void Verify()
        {
            if (!Directory.Exists(Path.GetDirectoryName(Globals.AuthDataPath))) Directory.CreateDirectory(Path.GetDirectoryName(Globals.AuthDataPath) ?? throw new InvalidOperationException());

            if (!File.Exists(Globals.AuthDataPath)) Write(new AuthResponse());
        }

        public static void Write(AuthResponse authResponse)
        {
            var json = JsonSerializer.Serialize(authResponse);

            var encryptedData = Aes.Encrypt(json);

            File.WriteAllText(Globals.AuthDataPath, encryptedData);
        }

        public static AuthResponse Read()
        {
            Verify();

            var encryptedData = File.ReadAllText(Globals.AuthDataPath);

            var decryptedContent = Aes.Decrypt(encryptedData);

            return JsonSerializer.Deserialize<AuthResponse>(decryptedContent);
        }
    }
}