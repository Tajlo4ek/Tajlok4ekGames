using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ClientServer
{
    public static class TokenGenerator
    {
        private static readonly List<string> userToken = new List<string>();

        public static string Generate(int size)
        {
            var charSet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string token;

            do
            {
                var chars = charSet.ToCharArray();
                var data = new byte[1];
                var crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                var result = new StringBuilder(size);
                foreach (var b in data)
                {
                    result.Append(chars[b % chars.Length]);
                }

                token = result.ToString();
            }
            while (userToken.Contains(token));

            userToken.Add(token);

            return token;
        }
    }

}
