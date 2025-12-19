namespace Extensions
{
    public static class StringExtensions
    {
        public static string AddIfNonEmpty(this string s, string toAdd)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s + toAdd;
        }
        
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}