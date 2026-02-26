#if UNITY_EDITOR
using System.Collections.Generic;
using Common.Currency;
using Storage.Snapshots;

namespace Editor.Tests
{
    /// <summary>
    /// Test service that builds <see cref="ProfileSnapshot"/> instances for unit tests.
    /// </summary>
    public class TestProfileSnapshotBuilderService
    {
        /// <summary>
        /// Builds a minimal profile snapshot with default currencies, empty packs, and optional initial daily reward state.
        /// </summary>
        public ProfileSnapshot BuildMinimal(DailyRewardSnapshot initialDailyReward = null)
        {
            var snapshot = new ProfileSnapshot(
                new Stars(0),
                new SoftCurrency(0),
                new HardCurrency(0),
                new List<PackSnapshot>(),
                new List<string>(),
                new AnalyticsInfoSnapshot(0, 0, 0),
                new GameParamsSnapshot(true, true, "en"));
            snapshot.DailyRewardSnapshot = initialDailyReward;
            return snapshot;
        }
    }
}
#endif
