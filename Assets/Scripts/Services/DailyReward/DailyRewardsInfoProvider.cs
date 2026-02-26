using System;
using System.Linq;
using Common.Currency;
using Configurations.DailyReward;
using Services.Configuration;
using Services.Player;
using Storage.Snapshots;
using Zenject;

namespace Services.DailyReward
{
    /// <summary>
    /// Provides all read-only info for the daily reward feature (preview, popup data, status).
    /// </summary>
    public sealed class DailyRewardsInfoProvider
    {
        [Inject] private readonly PlayerProfileController _playerProfileController;
        [Inject] private readonly GameConfigurationProvider _configurationProvider;
        [Inject] private readonly BridgeService _bridgeService;

        private DailyRewardConfiguration _config;
        private DailyRewardConfiguration Config => _config ??= _configurationProvider.GetConfig<DailyRewardConfiguration>();

        public bool TryGetTodayRewardPreview(out DailyRewardInfo rewardInfo)
        {
            rewardInfo = default;
            if (!_playerProfileController.IsInitialized)
                return false;

            var ctx = GetContext();
            if (ctx.isClaimedToday)
                return false;

            int nextDay = CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimDate, ctx.today);
            var rewards = Config?.GetRewardsForDay(nextDay);

            if (rewards == null || rewards.Count == 0)
                return false;

            rewardInfo = new DailyRewardInfo(nextDay, Config.RewardsByDay, rewards);
            return true;
        }

        public bool TryGetDailyRewardPopupInfo(out DailyRewardInfo info)
        {
            if (TryGetTodayRewardPreview(out info)) return true;
            if (!_playerProfileController.IsInitialized || Config?.RewardsByDay == null) return false;

            var ctx = GetContext();
            int displayDay = ctx.isClaimedToday
                ? ctx.snapshot.CurrentDayIndex
                : CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimDate, ctx.today);

            info = new DailyRewardInfo(displayDay, Config.RewardsByDay,
                Config.GetRewardsForDay(displayDay) ?? Array.Empty<ICurrency>());
            return true;
        }

        public DailyRewardStatus GetRewardStatus()
        {
            if (!_playerProfileController.IsInitialized) return new DailyRewardStatus(false, false, TimeSpan.Zero, 0);

            var ctx = GetContext();
            var isMissed = ctx.lastClaimDate < ctx.today.AddDays(-1) && ctx.snapshot.LastClaimUtcTicks > 0;
            var timeUntilNext = ctx.isClaimedToday ? ctx.today.AddDays(1) - _bridgeService.GetServerTime() : TimeSpan.Zero;

            return new DailyRewardStatus(!ctx.isClaimedToday, isMissed, timeUntilNext, ctx.snapshot.CurrentDayIndex);
        }

        public bool IsCollectedDay(int dayIndex)
        {
            if (!_playerProfileController.IsInitialized)
                return false;
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot();
            if (snapshot?.ClaimedDaysIndexes == null || snapshot.ClaimedDaysIndexes.Count == 0)
                return false;
            return snapshot.ClaimedDaysIndexes.Contains(dayIndex);
        }

        internal int CalculateNextDayIndex(int current, DateTime lastClaim, DateTime today)
        {
            bool isStreakBroken = lastClaim < today.AddDays(-1);
            if (isStreakBroken || lastClaim == DateTime.MinValue) return 1;

            return (current % DailyRewardConfiguration.CycleLength) + 1;
        }

        internal (DailyRewardSnapshot snapshot, DateTime today, DateTime lastClaimDate, bool isClaimedToday) GetContext()
        {
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);

            var today = _bridgeService.GetServerTime().ToUniversalTime().Date;

            var lastClaimDate = snapshot.LastClaimUtcTicks switch
            {
                > 0 => new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc).Date,
                _ => DateTime.MinValue
            };

            if (IsStreakBroken(lastClaimDate, today) && !IsSnapshotReset(snapshot))
            {
                var resetSnapshot = new DailyRewardSnapshot(1, snapshot.LastClaimUtcTicks, null);
                _playerProfileController.UpdateDailyRewardAndSave(resetSnapshot, SavePriority.ImmediateSave);
                return (resetSnapshot, today, lastClaimDate, lastClaimDate == today);
            }

            return (snapshot, today, lastClaimDate, lastClaimDate == today);
        }

        private static bool IsStreakBroken(DateTime lastClaimDate, DateTime today)
        {
            return lastClaimDate == DateTime.MinValue || lastClaimDate < today.AddDays(-1);
        }

        private static bool IsSnapshotReset(DailyRewardSnapshot snapshot)
        {
            return snapshot.CurrentDayIndex == 1 && (snapshot.ClaimedDaysIndexes == null || snapshot.ClaimedDaysIndexes.Count == 0);
        }
    }
}
