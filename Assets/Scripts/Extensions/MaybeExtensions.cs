using System;

namespace Extensions
{
    public static class MaybeExtensions
    {
        public static bool May(this bool value, Action action)
        {
            if (value)
            {
                action();
            }
            return value;
        }

        public static bool May(this object value, Action action)
        {
            var hasValue = value != null;
            return hasValue.May(action);
        }

        public static bool May<T>(this T value, Action<T> action)
        {
            var hasValue = value != null;
            return hasValue.May(() => action(value));
        }

        public static TOut May<T, TOut>(this T value, Func<T, TOut> transform)
        {
            var hasValue = value != null;
            if (hasValue)
            {
                return transform(value);
            }

            return default(TOut);
        }

        public static T Or<T>(this T value, T defaultValue)
        {
            var hasValue = value != null;
            return hasValue ? value : defaultValue;
        }

        public static int TryGetHashCode<T>(this T value)
        {
            return value != null ? value.GetHashCode() : 0;
        }
    }
}