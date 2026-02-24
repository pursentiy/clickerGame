using System;

namespace Services.DailyReward
{
    public readonly struct DailyRewardStatus
    {
        public readonly bool IsAvailable;
        public readonly bool IsMissed;
        public readonly TimeSpan TimeUntilNext;
        public readonly int CurrentDayIndex;

        public DailyRewardStatus(bool isAvailable, bool isMissed, TimeSpan timeUntilNext, int currentDayIndex)
        {
            IsAvailable = isAvailable;
            IsMissed = isMissed;
            TimeUntilNext = timeUntilNext;
            CurrentDayIndex = currentDayIndex;
        }
    }
}
