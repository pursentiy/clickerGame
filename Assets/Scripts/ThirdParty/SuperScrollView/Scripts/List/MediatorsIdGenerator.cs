namespace ThirdParty.SuperScrollView.Scripts.List
{
    public static class MediatorsIdGenerator
    {
        private static int _id;

        public static int Next()
        {
            return _id++;
        }
    }
}