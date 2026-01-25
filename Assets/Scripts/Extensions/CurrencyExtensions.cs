using Common.Currency;

namespace Extensions
{
    public static class CurrencyExtensions
    {
        public static string StarsCurrencyName => new Stars(0).GetType().Name;
    }
}