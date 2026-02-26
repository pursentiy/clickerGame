#if UNITY_EDITOR
using System;
using System.Linq;
using Configurations.DailyReward;
using Editor.Tests;
using NUnit.Framework;
using Services.DailyReward;
using Storage.Snapshots;

namespace Tests.Editor
{
    [TestFixture]
    public class DailyRewardClaimFlowTests
    {
        private const int CycleLength = 6;

        private readonly TestBuilderService _builder = new TestBuilderService();

        [Test]
        public void ClaimRewardsEveryDay_AfterSixAttempts_HasSixRewardsMatchingConfig_ClaimedDaysIndexesHasSixDays_CurrentDayIndexIsSix()
        {
            var today = DateTime.UtcNow.Date;
            var ctx = _builder.BuildDailyRewardFlowContext(today);

            for (int day = 1; day <= CycleLength; day++)
            {
                bool claimed = ctx.Controller.TryClaimTodayReward();
                Assert.IsTrue(claimed, $"Claim for day {day} should succeed.");

                DailyRewardSnapshot snapshot = ctx.ProfileController.TryGetDailyRewardSnapshot();
                Assert.IsNotNull(snapshot, "Snapshot should not be null after claim.");
                Assert.AreEqual(day, snapshot.CurrentDayIndex, $"After claim {day}, CurrentDayIndex should be {day}.");
                Assert.AreEqual(day, snapshot.ClaimedDaysIndexes?.Count ?? 0, $"After claim {day}, ClaimedDaysIndexes should have {day} entries.");

                var expectedForDay = ctx.Config.GetRewardsForDay(day);
                Assert.IsNotNull(expectedForDay, $"Config should have rewards for day {day}.");
                Assert.Greater(expectedForDay.Count, 0f, $"Config should have at least one reward for day {day}.");

                ctx.Bridge.AdvanceDays(1);
            }

            DailyRewardSnapshot finalSnapshot = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(finalSnapshot);
            Assert.AreEqual(CycleLength, finalSnapshot.CurrentDayIndex, "After 6 claims, CurrentDayIndex should be 6.");
            Assert.AreEqual(CycleLength, finalSnapshot.ClaimedDaysIndexes?.Count ?? 0, "ClaimedDaysIndexes should have 6 days.");
            Assert.That(finalSnapshot.ClaimedDaysIndexes, Is.EquivalentTo(Enumerable.Range(1, CycleLength).ToList()),
                "ClaimedDaysIndexes should contain 1,2,3,4,5,6.");

            for (int day = 1; day <= CycleLength; day++)
            {
                var rewardsForDay = ctx.Config.GetRewardsForDay(day);
                Assert.IsNotNull(rewardsForDay, $"Configuration must define rewards for day {day}.");
                Assert.Greater(rewardsForDay.Count, 0, $"Day {day} should have at least one reward in config.");
                Assert.IsTrue(finalSnapshot.ClaimedDaysIndexes.Contains(day), $"ClaimedDaysIndexes should contain day {day}.");
            }
        }
    }
    
    public sealed class DailyRewardFlowContext
    {
        public TestBridgeService Bridge { get; }
        public DailyRewardConfiguration Config { get; }
        public TestPlayerProfileController ProfileController { get; }
        public DailyRewardsInfoProvider InfoProvider { get; }
        public DailyRewardController Controller { get; }

        public DailyRewardFlowContext(
            TestBridgeService bridge,
            DailyRewardConfiguration config,
            TestPlayerProfileController profileController,
            DailyRewardsInfoProvider infoProvider,
            DailyRewardController controller)
        {
            Bridge = bridge;
            Config = config;
            ProfileController = profileController;
            InfoProvider = infoProvider;
            Controller = controller;
        }
    }
}
#endif
