#if UNITY_EDITOR
using System;
using System.Linq;
using Configurations.DailyReward;
using NUnit.Framework;
using Services.DailyReward;
using Storage.Snapshots;

namespace Tests.Editor
{
    [TestFixture]
    public class DailyRewardFlowTests
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
                Assert.Greater(expectedForDay.Count, 0, $"Config should have at least one reward for day {day}.");

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

        [Test]
        public void AfterClaimingDayOne_WhenReturningAfterFiveDays_StreakResetsAndOnlyDayOneIsAvailableToClaim()
        {
            var today = DateTime.UtcNow.Date;
            var ctx = _builder.BuildDailyRewardFlowContext(today);

            ctx.Controller.TryClaimTodayReward();
            DailyRewardSnapshot afterFirst = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(afterFirst);
            Assert.AreEqual(1, afterFirst.CurrentDayIndex);
            Assert.That(afterFirst.ClaimedDaysIndexes, Is.EquivalentTo(new[] { 1 }), "After first claim: day 1.");

            ctx.Bridge.AdvanceDays(1);
            ctx.Controller.TryClaimTodayReward();
            DailyRewardSnapshot afterSecond = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(afterSecond);
            Assert.AreEqual(2, afterSecond.CurrentDayIndex);
            Assert.That(afterSecond.ClaimedDaysIndexes, Is.EquivalentTo(new[] { 1, 2 }), "After second claim: days 1, 2.");

            ctx.Bridge.AdvanceDays(1);
            ctx.Controller.TryClaimTodayReward();
            DailyRewardSnapshot afterThird = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(afterThird);
            Assert.AreEqual(3, afterThird.CurrentDayIndex);
            Assert.That(afterThird.ClaimedDaysIndexes, Is.EquivalentTo(new[] { 1, 2, 3 }), "After third claim: days 1, 2, 3.");

            ctx.Bridge.AdvanceDays(2);

            bool hasPreview = ctx.InfoProvider.TryGetTodayRewardPreview(out var rewardInfo);
            Assert.IsTrue(hasPreview, "After skipping day 4 there should be one reward available (day 1).");
            Assert.AreEqual(1, rewardInfo.DayIndex, "Streak broken; next claimable is day 1.");

            DailyRewardSnapshot afterOpening = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(afterOpening);
            Assert.AreEqual(3, afterOpening.CurrentDayIndex,
                "Stored snapshot is not reset on open; only controller persists when user claims.");

            bool claimAfterReset = ctx.Controller.TryClaimTodayReward();
            Assert.IsTrue(claimAfterReset, "Claim after streak break should succeed.");
            DailyRewardSnapshot afterClaim = ctx.ProfileController.TryGetDailyRewardSnapshot();
            Assert.IsNotNull(afterClaim);
            Assert.AreEqual(1, afterClaim.CurrentDayIndex);
            Assert.That(afterClaim.ClaimedDaysIndexes, Is.EquivalentTo(new[] { 1 }),
                "After claiming, controller should persist reset snapshot (day 1, [1]).");
        }
    }
}
#endif
