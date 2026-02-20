using System;

namespace Common.Currency
{
    public static class CurrencyParser
    {
        /// <summary>
        /// Parses a string like "Stars 5" or "HardCurrency 10" into ICurrency.
        /// Returns null if parsing fails.
        /// </summary>
        public static ICurrency Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var parts = value.Trim().Split(' ');
            if (parts.Length != 2)
            {
                if (int.TryParse(value, out int legacyAmount))
                    return new Stars(legacyAmount);
                return null;
            }

            if (!Enum.TryParse<CurrencyType>(parts[0], true, out var currencyType))
                return null;

            if (!int.TryParse(parts[1], out int amount))
                return null;

            return CreateCurrency(currencyType, amount);
        }

        public static ICurrency CreateCurrency(CurrencyType currencyType, int amount)
        {
            return currencyType switch
            {
                CurrencyType.Stars => new Stars(amount),
                CurrencyType.HardCurrency => new HardCurrency(amount),
                CurrencyType.SoftCurrency => new SoftCurrency(amount),
                _ => null
            };
        }

        public static CurrencyType GetCurrencyType(ICurrency currency)
        {
            return currency switch
            {
                Stars => CurrencyType.Stars,
                HardCurrency => CurrencyType.HardCurrency,
                SoftCurrency => CurrencyType.SoftCurrency,
                _ => CurrencyType.None
            };
        }
    }
}
