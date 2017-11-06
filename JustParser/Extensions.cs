using System;
using System.Collections.Generic;

namespace JustParser
{
    public static class Extensions
    {
        public static OrderedDictionary<TK, TV> ToOrderedDictionary<T, TK, TV>(this IEnumerable<T> src, Func<T, TK> keyFunc, Func<T, TV> valFunc) where TK : IEquatable<TK>
        {
            var orderedDictionary = new OrderedDictionary<TK, TV>();
            foreach (var item in src)
            {
                var key = keyFunc(item);
                var value = valFunc(item);
                orderedDictionary.Add(key, value);
            }
            return orderedDictionary;
        }
    }
}