using System;
using System.Collections.Generic;
using Common.Currency;

namespace Storage.Records
{
    [Serializable]
    public class ProfileRecord
    {
        public ProfileRecord(int stars, int softCurrency, int hardCurrency, List<PackRecord> packRecords, AnalyticsInfoRecord analyticsInfoRecord)
        {
            Stars = stars;
            PackRecords = packRecords;
            AnalyticsInfoRecord = analyticsInfoRecord;
            SoftCurrency = softCurrency;
            HardCurrency = hardCurrency;
        }
        
        public int Version = 1; 
        
        public int Stars;
        public int SoftCurrency; // Например, монеты
        public int HardCurrency; // Например, кристаллы
        
        public List<PackRecord> PackRecords;
        public List<string> PurchasedItemsIds;
        public AnalyticsInfoRecord AnalyticsInfoRecord;
    }
}