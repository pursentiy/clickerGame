using System;

namespace Storage.Records
{
    //TODO ANALYTICS
    [Serializable]
    public class AnalyticsInfoRecord
    {
        public AnalyticsInfoRecord(long lastSaveTime, double totalPlayTime, long creationDate)
        {
            LastSaveTime = lastSaveTime;
            TotalPlayTime = totalPlayTime;
            CreationDate = creationDate;
        }
        
        public long LastSaveTime;
        public double TotalPlayTime;
        public long CreationDate;
    }
}