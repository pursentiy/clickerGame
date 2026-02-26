using System;
using System.Collections.Generic;

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
        public List<int> ClaimedDaysIndexes;

        public DailyRewardRecord(int currentDayIndex, long lastClaimUtcTicks, List<int> claimedDaysIndexes = null)
        {
            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = lastClaimUtcTicks;
            ClaimedDaysIndexes = claimedDaysIndexes ?? new List<int>();
        }
    }
}

