namespace Storage.Snapshots
{
    public class AnalyticsInfoSnapshot
    {
        public AnalyticsInfoSnapshot(long lastSaveTime, double totalPlayTime, long creationDate)
        {
            LastSaveTime = lastSaveTime;
            TotalPlayTime = totalPlayTime;
            CreationDate = creationDate;
        }
        
        public long LastSaveTime { get; private set;}
        public double TotalPlayTime { get; private set;}
        public long CreationDate { get; private set;}
    }
}