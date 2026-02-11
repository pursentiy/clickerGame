using System.Collections.Generic;
using Common.Currency;
using Handlers.UISystem.Popups;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardPopupContext : IPopupContext
    {
        public DailyRewardPopupContext(
            int dayIndex,
            IReadOnlyDictionary<int, IList<ICurrency>> rewardsByDay,
            IList<ICurrency> earnedDailyReward)
        {
            DayIndex = dayIndex;
            RewardsByDay = rewardsByDay;
            EarnedDailyReward = earnedDailyReward;
        }

        public int DayIndex { get; }
        public IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay { get; }
        public IList<ICurrency> EarnedDailyReward { get; }
    }
}

