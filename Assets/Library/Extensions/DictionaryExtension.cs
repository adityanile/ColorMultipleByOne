//Resharper disable all 
using System;
using System.Linq;
using System.Collections.Generic;

namespace Library.Extensions{
    public static class DictionaryExtension{
        public static void AddSafe<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, TV value) {
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        public static TV GetSafe<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, TV defValue) {
            return dictionary.TryGetValue(key, out var v) ? v : defValue;
        }

        public static Dictionary<TKey, TValue> Shuffle<TKey, TValue>(
            this Dictionary<TKey, TValue> source) {
            var r = new Random();
            return source.OrderBy(x => r.Next())
                .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}