using System;
using System.Collections.Generic;
using System.Linq;

namespace JustParser
{
    public static class CharArrayExtensions
    {
        public static bool IsDigits(this ArraySegment<char> chars)
        {
            if (chars.Count == 0) return false;

            foreach (var c in chars)
            {
                if (!char.IsDigit(c)) return false;
            }

            return true;
        }

        public static IEnumerable<int> IndexesOf(this ArraySegment<char> chars, char splitChar)
        {
            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[i];
                if (ch == splitChar)
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<ArraySegment<char>> Split(this ArraySegment<char> chars, char splitChar)
        {
            if(chars.Count == 0) yield break;

            var start = 0;

            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[i];
                if (ch == splitChar)
                {
                    var c = chars.Slice(start, i - start);
                    yield return c;
                    start = i + 1;
                }
            }

            var last = chars.Slice(start);
            yield return last;
        }


        public static ArraySegment<char> TrimStart(this ArraySegment<char> chars)
        {
            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i] != ' ')
                {
                    return chars.Slice(i);
                }
            }

            return ArraySegment<char>.Empty;
        }

        public static bool Contains(this ArraySegment<char> chars, char ch)
        {
            return chars.IndexesOf(ch).Any();
        }

        public static bool IsOneOf(this ArraySegment<char> chars, IEnumerable<string> strings)
        {
            foreach (var s in strings)
            {
                if (chars.SequenceEqual(s)) return true;
            }

            return false;
        }

        public static bool IsEqualsTo(this IEnumerable<char> chars, IEnumerable<char> chars2)
        {
            return chars.SequenceEqual(chars2);
        }

        public static string AsString(this ArraySegment<char> chars)
        {
            return new String(chars.ToArray());
        }

        //public static bool Contains(this ArraySegment<char> chars, string str)
        //{
        //    var ar = str.ToCharArray();
        //    for (int i = 0; i < chars.Count - ar.Length; i++)
        //    {
        //        var match = true;
        //        for (int j = 0; j < ar.Length; j++)
        //        {

        //        }
        //    }
        //}
    }
}