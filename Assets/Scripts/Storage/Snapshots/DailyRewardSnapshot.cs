namespace Storage.Snapshots
{
    /// <summary>
    /// Immutable runtime representation of daily reward progression.
    /// </summary>
    public sealed class DailyRewardSnapshot
    {
        public static DailyRewardSnapshot Default => new DailyRewardSnapshot(0, 0);
        
        public DailyRewardSnapshot(int currentDayIndex, long lastClaimUtcTicks)
        {
            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = lastClaimUtcTicks;
        }

        public int CurrentDayIndex { get; }
        public long LastClaimUtcTicks { get; }
    }
}

