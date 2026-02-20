using Common.Currency;

namespace Extensions
{
    public static class CurrencyExtensions
    {
        public static string StarsCurrencyName => nameof(Stars);
        public static string HardCurrencyName => nameof(HardCurrency);
        public static string SoftCurrencyName => nameof(SoftCurrency);

        public static string GetCurrencyName(ICurrency currency)
        {
            if (currency == null) return StarsCurrencyName;
            return currency.GetType().Name;
        }

        public static string GetCurrencyName(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.Stars => StarsCurrencyName,
                CurrencyType.HardCurrency => HardCurrencyName,
                CurrencyType.SoftCurrency => SoftCurrencyName,
                _ => StarsCurrencyName
            };
        }
    }
}