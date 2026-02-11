namespace Storage.Snapshots
{
    /// <summary>
    /// Runtime representation of daily reward progression.
    /// </summary>
    public class DailyRewardSnapshot
    {
        public DailyRewardSnapshot(int currentDayIndex, long lastClaimUtcTicks)
        {
            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = lastClaimUtcTicks;
        }

        public int CurrentDayIndex { get; set; }
        public long LastClaimUtcTicks { get; set; }
    }
}

