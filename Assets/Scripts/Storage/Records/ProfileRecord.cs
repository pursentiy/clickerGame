using System;
using System.Collections.Generic;

namespace Storage.Records
{
    [Serializable]
    public class ProfileRecord
    {
        public ProfileRecord(
            int stars,
            int softCurrency,
            int hardCurrency,
            List<PackRecord> packRecords,
            AnalyticsInfoRecord analyticsInfoRecord,
            GameParamsRecord gameParamsRecord,
            DailyRewardRecord dailyRewardRecord = null)
        {
            Stars = stars;
            PackRecords = packRecords;
            AnalyticsInfoRecord = analyticsInfoRecord;
            GameParamsRecord = gameParamsRecord;
            SoftCurrency = softCurrency;
            HardCurrency = hardCurrency;
            DailyRewardRecord = dailyRewardRecord;
        }
         
        public int Stars;
        public List<PackRecord> PackRecords;
        
        //TODO IN THE FUTURE BLOCK
        public int Version = 1;
        public int SoftCurrency;
        public int HardCurrency;
        public List<string> PurchasedItemsIds;
        public AnalyticsInfoRecord AnalyticsInfoRecord;
        public GameParamsRecord GameParamsRecord;
        
        // Daily reward progression and last claim info
        public DailyRewardRecord DailyRewardRecord;
    }
}