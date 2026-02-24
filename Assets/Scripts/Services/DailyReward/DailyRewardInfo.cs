using System.Collections.Generic;
using Common.Currency;

namespace Services.DailyReward
{
    public readonly struct DailyRewardInfo
    {
        public readonly int DayIndex;
        public readonly IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay;
        public readonly IList<ICurrency> EarnedDailyReward;

        public DailyRewardInfo(
            int dayIndex,
            IReadOnlyDictionary<int, IList<ICurrency>> rewardsByDay,
            IList<ICurrency> earnedDailyReward)
        {
            DayIndex = dayIndex;
            RewardsByDay = rewardsByDay;
            EarnedDailyReward = earnedDailyReward;
        }
    }
}
