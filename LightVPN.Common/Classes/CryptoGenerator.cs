using System;
using System.Security.Cryptography;
using System.Text;

namespace LightVPN.Common
{
    public static class CryptoGenerator
    {
        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetCryptoString(int size)
        {
            byte[] data = new byte[4 * size];
            using RNGCryptoServiceProvider crypto = new();
            crypto.GetBytes(data);
            StringBuilder result = new(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }
}