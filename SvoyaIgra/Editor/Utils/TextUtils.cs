using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.Utils
{
    public static class TextUtils
    {
        static readonly string[] rusLet = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ы", "Э", "Ю", "Я" };
        static readonly string[] engLet = { "A", "B", "V", "G", "D", "Е", "E", "J", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "H", "C", "CH", "SH", "SH", "Y", "E", "YU", "YA" };

        static readonly string[] removeLet = { "\\", "/", ":", "*", "?", "\"", "<", ">", "|", "+", " ", ".", "%", "!", "@", };

        public static int IndexOf<T>(this T[] input, T value)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string Transliteration(string input)
        {
            StringBuilder sb = new StringBuilder(input);
            sb.Replace(" ", "_");

            foreach (var str in removeLet)
            {
                sb.Replace(str, "");
            }

            input = sb.ToString();
            sb.Clear();

            foreach (var ch in input)
            {
                var ind = rusLet.IndexOf(char.ToUpper(ch).ToString());

                if (ind != -1)
                {
                    var let = engLet[ind];

                    if (char.IsUpper(ch))
                    {
                        sb.Append(let);
                    }
                    else
                    {
                        sb.Append(let.ToLower());
                    }
                }
                else
                {
                    if (ch < 128)
                    {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }


    }
}
