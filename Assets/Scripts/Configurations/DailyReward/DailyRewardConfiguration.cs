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
    /// Day;StarsAmount
    /// 1;5
    /// 2;7
    /// ...
    /// 7;25
    /// </summary>
    public class DailyRewardConfiguration : ICSVConfig
    {
        public const int CycleLength = 7;

        /// <summary>
        /// Raw mapping of day index (1..7) to list of rewards for that day.
        /// Currently populated with Stars currency instances based on CSV.
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

            // Expect header in the first line
            for (var i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(';');
                if (values.Length < 2)
                    continue;

                try
                {
                    var dayIndex = int.Parse(values[0]);
                    var starsAmount = int.Parse(values[1]);

                    if (dayIndex <= 0)
                        continue;

                    // For now we support only Stars rewards loaded from CSV.
                    // Each day can contain a list of currencies; we create a single Stars entry.
                    if (!rewards.TryGetValue(dayIndex, out var list))
                    {
                        list = new List<ICurrency>();
                        rewards[dayIndex] = list;
                    }

                    list.Add(new Stars(starsAmount));
                }
                catch (Exception e)
                {
                    LoggerService.LogError($"[{nameof(DailyRewardConfiguration)}] Error on line {i}: {e.Message}");
                }
            }

            RewardsByDay = rewards;
        }

        public IList<ICurrency> GetRewardsForDay(int dayIndex)
        {
            if (RewardsByDay == null || RewardsByDay.Count == 0)
                return Array.Empty<ICurrency>();

            if (dayIndex <= 0)
                dayIndex = 1;

            // Clamp into configured range if needed
            if (RewardsByDay.TryGetValue(dayIndex, out var value) && value != null)
                return value;

            return Array.Empty<ICurrency>();
        }
    }
}

