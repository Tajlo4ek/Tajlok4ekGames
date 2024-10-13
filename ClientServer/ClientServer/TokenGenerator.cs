using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ClientServer
{
    public interface IHasToken
    {
        string Token { get; }
    }

    public static class TokenGenerator
    {
        private static readonly List<string> usedToken = new List<string>();

        public static string Generate(int size = 20)
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
            while (usedToken.Contains(token));

            usedToken.Add(token);

            return token;
        }
    }

}
