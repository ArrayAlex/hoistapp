using System;
using System.Security.Cryptography;
using System.Text;

namespace hoistmt.Services
{
    public static class TokenGenerator
    {
        private static readonly char[] chars = 
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GenerateToken(int length = 32)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);
                StringBuilder result = new StringBuilder(length);

                foreach (byte b in data)
                {
                    result.Append(chars[b % chars.Length]);
                }

                return result.ToString();
            }
        }
    }
}