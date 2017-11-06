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

        public static int IndexOf(this ArraySegment<char> chars, char c)
        {
            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[i];
                if (ch == c)
                {
                    return i;
                }
            }

            return -1;
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

        public static bool IsOneOf(this IReadOnlyCollection<char> chars, IEnumerable<string> strings)
        {
            return strings.Any(s => chars.Count == s.Length && chars.SequenceEqual(s));
        }

        public static bool IsEqualsTo(this IEnumerable<char> chars, IEnumerable<char> chars2)
        {
            return chars.SequenceEqual(chars2);
        }

        public static bool IsEqualsTo(this ICollection<char> chars, ICollection<char> chars2)
        {
            return chars.Count == chars2.Count && chars.SequenceEqual(chars2);
        }

        public static bool StartsWith(this ArraySegment<char> chars, ArraySegment<char> subCollection)
        {
            if (subCollection.Count > chars.Count) return false;
            return !chars.Where((t, i) => t != subCollection[i]).Any();
        }

        public static string AsString(this IEnumerable<char> chars)
        {
            return new String(chars.ToArray());
        }
    }
}