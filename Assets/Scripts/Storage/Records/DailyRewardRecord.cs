using System;

namespace Storage.Records
{
    /// <summary>
    /// Persistent data for daily login rewards.
    /// </summary>
    [Serializable]
    public class DailyRewardRecord
    {
        public int CurrentDayIndex;
        public long LastClaimUtcTicks;

        public DailyRewardRecord(int currentDayIndex, long lastClaimUtcTicks)
        {
            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = lastClaimUtcTicks;
        }
    }
}

