using System;
using System.Collections.Generic;
using System.Linq;
using Services;

namespace Extensions
{
    public static class CollectionExtensions
    {
        private static readonly System.Random _rng = new System.Random();
        private static readonly object _lock = new object();

        public static int[] ShuffleCopy(int[] source)
        {
            if (source == null) return null; 
            
            var array = (int[])source.Clone();
            var n = array.Length;
        
            while (n > 1)
            {
                n--;
                int k;
            
                lock (_lock)
                {
                    k = _rng.Next(n + 1);
                }

                (array[k], array[n]) = (array[n], array[k]);
            }

            return array;
        }
        
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
        
        public static bool IsCollectionNullOrEmpty<T>(this IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
        
        public static bool InListRange(this int i, System.Collections.IList list)
        {
            if (list == null || list.Count == 0)
                return false;
            
            return i >= 0 && i < list.Count;
        }
        
        public static void Foreach<T>(this List<T> list, Action<T> action)
        {
            if (list == null) return;

            for (var i = 0; i < list.Count; i++)
            {
                action.SafeInvoke(list[i]);
            }
        }
        
        public static void Foreach<T>(this T[] array, Action<T> action)
        {
            if (array == null) return;
			
            for (int i = 0; i < array.Length; i++)
            {
                action.SafeInvoke(array[i]);
            }
        }
        
        public static IList<TOut> Map<TIn, TOut>(this IEnumerable<TIn> container, Func<TIn, TOut> mapFunction,
            IList<TOut> buffer = null)
        {
            buffer = buffer.Or(new List<TOut>());
			
            foreach (var inValue in container)
            {
                var transformed = mapFunction(inValue);
                if (transformed != null)
                {
                    buffer.Add(transformed);
                }
            }

            return buffer;
        }
        
        public static TValue GetValueOrFirst<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (key != null)
            {
                try
                {
                    if (dictionary.ContainsKey(key))
                    {
                        return dictionary[key];
                    }
                }
                catch (Exception e)
                {
                    LoggerService.LogWarning($"{e} with a {key} in the dictionary");
                }
            }

            return dictionary.Any() ? dictionary.Values.First() : default;
        }
        
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}