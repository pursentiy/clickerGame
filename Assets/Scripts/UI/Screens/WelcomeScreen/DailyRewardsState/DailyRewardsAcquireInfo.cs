using System.Collections.Generic;
using Common.Currency;

namespace UI.Screens.WelcomeScreen.DailyRewardsState
{
    public class DailyRewardsAcquireInfo
    {
        public DailyRewardsAcquireInfo(IList<ICurrency> earnedDailyReward)
        {
            EarnedDailyReward = earnedDailyReward;
        }

        public IList<ICurrency> EarnedDailyReward { get; }
    }
}