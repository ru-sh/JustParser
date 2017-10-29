using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CheapFlights.Importer.Lexing
{
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        private readonly List<KeyValuePair<TKey, TValue>> _list = new List<KeyValuePair<TKey, TValue>>();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new Exception("This key already in collection.");
            }

            _list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return _list.Any(pair => object.Equals(pair.Key, key));
        }

        public bool Remove(TKey key)
        {
            var i = _list.FindIndex(p => Equals(p.Key, key));
            if (i < 0) return false;

            _list.RemoveAt(i);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var item = _list.FirstOrDefault(pair => Equals(pair.Key, key));
            value = item.Value;
            return Equals(item.Key, key); // found
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue val))
                {
                    return val;
                }

                throw new Exception("Not able to find item by key.");
            }
            set
            {
                var i = _list.FindIndex(p => Equals(p.Key, key));
                var pair = new KeyValuePair<TKey, TValue>(key, value);
                if (i >= 0)
                {
                    _list[i] = pair;
                }
                else
                {
                    _list.Add(pair);
                }
            }
        }

        public ICollection<TKey> Keys => _list.Select(pair => pair.Key).ToList();
        public ICollection<TValue> Values => _list.Select(pair => pair.Value).ToList();
    }
}