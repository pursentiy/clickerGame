using System.Collections.Generic;
using Common.Currency;

namespace Storage.Snapshots
{
    public class ProfileSnapshot
    {
        public ProfileSnapshot(
            Stars stars,
            SoftCurrency softCurrency,
            HardCurrency hardCurrency,
            List<PackSnapshot> packSnapshots,
            List<string> purchasedItemsIds,
            AnalyticsInfoSnapshot analyticsInfoSnapshot,
            GameParamsSnapshot gameParamsSnapshot)
        {
            Stars = stars;
            PackSnapshots = packSnapshots;
            SoftCurrency = softCurrency;
            HardCurrency = hardCurrency;
            PurchasedItemsIds = purchasedItemsIds;
            AnalyticsInfoSnapshot = analyticsInfoSnapshot;
            GameParamsSnapshot = gameParamsSnapshot;
        }

        public Stars Stars {get; set;}
        public SoftCurrency SoftCurrency {get; set;}
        public HardCurrency HardCurrency {get; set;}
        public List<PackSnapshot> PackSnapshots {get; set;}
        public List<string> PurchasedItemsIds {get; set;}
        public AnalyticsInfoSnapshot AnalyticsInfoSnapshot {get; set;}
        public GameParamsSnapshot GameParamsSnapshot {get; set;}
        
        // Daily reward runtime data
        public DailyRewardSnapshot DailyRewardSnapshot { get; set; }
    }
}