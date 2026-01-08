using System;

namespace Extensions
{
    public static class NumbersExtensions
    {
        public static void Times(this int count, Action<int> callback)
        {
            if (callback == null) 
                return;
            
            for (int i = 0; i < count; i++)
                callback.Invoke(i);
        }
    }
}