namespace Static
{
    public static class Extensions
    {
        private static readonly System.Random Random = new System.Random();
        
        public static int[] Shuffle(int[] array)
        {
            var p = array.Length;
            for (var n = p - 1; n > 0; n--)
            {
                var r = Random.Next(0, n);
                var t = array[r];
                array[r] = array[n];
                array[n] = t;
            }

            return array;
        }
    }
}