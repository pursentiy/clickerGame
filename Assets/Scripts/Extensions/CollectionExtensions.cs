using System.Collections.Generic;

namespace Extensions
{
    public static class CollectionExtensions
    {
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