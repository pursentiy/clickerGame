using System.Collections.Generic;
using Common.Currency;
using Handlers.UISystem.Popups;
using RSG;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardPopupContext : IPopupContext
    {
        private readonly Promise<bool> _claimedPromise;

        public DailyRewardPopupContext(
            int dayIndex,
            IReadOnlyDictionary<int, IList<ICurrency>> rewardsByDay,
            IList<ICurrency> earnedDailyReward,
            Promise<bool> claimedPromise)
        {
            DayIndex = dayIndex;
            RewardsByDay = rewardsByDay;
            EarnedDailyReward = earnedDailyReward;
            _claimedPromise = claimedPromise;
        }

        public int DayIndex { get; }
        public IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay { get; }
        public IList<ICurrency> EarnedDailyReward { get; }
        public Promise<bool> ClaimedRewards => _claimedPromise;
    }
}

