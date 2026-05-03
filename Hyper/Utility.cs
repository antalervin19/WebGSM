using System;
using System.Security.Cryptography;

namespace HyperUtility
{
    public class Utility
    {
        public static string GetRandomID(int length)
        {
            string digits = "0123456789abcdefghijklmnopqrstuvwxyz";
            char[] id = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);

                for (int i = 0; i < id.Length; i++)
                {
                    id[i] = digits[data[i] % digits.Length];
                }
            }

            return new string(id);
        }
    }
}