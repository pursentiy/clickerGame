using System.Collections.Generic;

namespace Storage.Snapshots
{
    /// <summary>
    /// Immutable runtime representation of daily reward progression.
    /// </summary>
    public sealed class DailyRewardSnapshot
    {
        public static DailyRewardSnapshot Default => new DailyRewardSnapshot(0, 0, null);

        public DailyRewardSnapshot(int currentDayIndex, long lastClaimUtcTicks, IReadOnlyList<int> claimedDaysIndexes = null)
        {
            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = lastClaimUtcTicks;
            ClaimedDaysIndexes = claimedDaysIndexes ?? (IReadOnlyList<int>)new List<int>();
        }

        public int CurrentDayIndex { get; }
        public long LastClaimUtcTicks { get; }
        public IReadOnlyList<int> ClaimedDaysIndexes { get; }
    }
}

