using System.Collections.Generic;

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
        
        public static bool InListRange(this int i, System.Collections.IList list)
        {
            if (list == null || list.Count == 0)
                return false;
            
            return i >= 0 && i < list.Count;
        }
    }
}