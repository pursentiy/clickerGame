using System;
using System.Collections.Generic;
using Common.Currency;
using Storage.Snapshots;

namespace Services
{
    public class ProfileBuilderService
    {
        public ProfileSnapshot BuildNewProfileSnapshot()
        {
            return new ProfileSnapshot(
                0, 
                new SoftCurrency(0), 
                new HardCurrency(0),
                new List<PackSnapshot>(),
                new List<string>(),
                new AnalyticsInfoSnapshot(0, 0, DateTime.UtcNow.Ticks));
        }
    }
}