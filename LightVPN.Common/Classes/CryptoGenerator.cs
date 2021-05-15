using System;
using System.Security.Cryptography;
using System.Text;

namespace LightVPN.Common
{
    /// <summary>
    /// Class that contains a cryptographically secure RNG-based method to get randomly generated strings
    /// </summary>
    public static class CryptoGenerator
    {
        /// <summary>
        /// Characters that the method will use to generate a random string
        /// </summary>
        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        /// <summary>
        /// Generates a random string to the size of <paramref name="size"/>
        /// </summary>
        /// <param name="size">The size you want the string to be</param>
        /// <returns>The randomly generated string</returns>
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