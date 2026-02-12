using System;
using System.Collections.Generic;
using Common.Currency;
using Configurations;
using Services;

namespace Configurations.DailyReward
{
    /// <summary>
    /// CSV-driven configuration for daily login rewards.
    /// File name must be DailyRewardConfiguration.csv and placed under Resources.
    /// Format:
    /// Day; Rewards
    /// 1; Stars 10
    /// 2; Stars 20, HardCurrency 30
    /// ...
    /// Rewards: comma-separated list of "CurrencyName Amount" (e.g. Stars 10, SoftCurrency 5).
    /// Supported currency names: Stars, HardCurrency, SoftCurrency.
    /// </summary>
    public class DailyRewardConfiguration : ICSVConfig
    {
        public const int CycleLength = 6;

        /// <summary>
        /// Raw mapping of day index (1..7) to list of rewards for that day.
        /// </summary>
        public IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay { get; private set; }

        public void Parse(string csvText)
        {
            var rewards = new Dictionary<int, IList<ICurrency>>();

            if (string.IsNullOrEmpty(csvText))
            {
                RewardsByDay = rewards;
                return;
            }

            var lines = csvText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var semicolonIndex = line.IndexOf(';');
                if (semicolonIndex < 0)
                    continue;

                var dayStr = line.Substring(0, semicolonIndex).Trim();
                var rewardsStr = line.Substring(semicolonIndex + 1).Trim();
                if (string.IsNullOrEmpty(rewardsStr))
                    continue;

                try
                {
                    var dayIndex = int.Parse(dayStr);
                    if (dayIndex <= 0)
                        continue;

                    var list = ParseRewardsLine(rewardsStr);
                    if (list != null && list.Count > 0)
                        rewards[dayIndex] = list;
                }
                catch (Exception e)
                {
                    LoggerService.LogError($"[{nameof(DailyRewardConfiguration)}] Error on line {i + 1}: {e.Message}");
                }
            }

            RewardsByDay = rewards;
        }

        /// <summary>
        /// Parses a single "Rewards" cell: comma-separated "CurrencyName Amount" (e.g. "Stars 10, HardCurrency 30").
        /// </summary>
        private static IList<ICurrency> ParseRewardsLine(string rewardsStr)
        {
            var list = new List<ICurrency>();
            var parts = rewardsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var lastSpace = trimmed.LastIndexOf(' ');
                if (lastSpace <= 0)
                    continue;

                var currencyName = trimmed.Substring(0, lastSpace).Trim();
                var amountStr = trimmed.Substring(lastSpace + 1).Trim();
                if (string.IsNullOrEmpty(currencyName) || string.IsNullOrEmpty(amountStr))
                    continue;

                if (!int.TryParse(amountStr, out var amount))
                    continue;

                var currency = CreateCurrency(currencyName, amount);
                if (currency != null)
                    list.Add(currency);
            }

            return list;
        }

        private static ICurrency? CreateCurrency(string name, int amount)
        {
            switch (name)
            {
                case "Stars":
                    return new Stars(amount);
                case "HardCurrency":
                    return new HardCurrency(amount);
                case "SoftCurrency":
                    return new SoftCurrency(amount);
                default:
                    LoggerService.LogError($"[{nameof(DailyRewardConfiguration)}] Unknown currency: '{name}'. Supported: Stars, HardCurrency, SoftCurrency.");
                    return null;
            }
        }

        public IList<ICurrency> GetRewardsForDay(int dayIndex)
        {
            if (RewardsByDay == null || RewardsByDay.Count == 0)
                return Array.Empty<ICurrency>();

            if (dayIndex <= 0)
                dayIndex = 1;

            if (RewardsByDay.TryGetValue(dayIndex, out var value) && value != null)
                return value;

            return Array.Empty<ICurrency>();
        }
    }
}

