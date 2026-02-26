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
            if (ctx.isClaimedInCurrentPeriod)
                return false;

            int nextDay = CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimUtc, ctx.nowUtc, ctx.intervalMinutes);
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
            int displayDay = ctx.isClaimedInCurrentPeriod
                ? ctx.snapshot.CurrentDayIndex
                : CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimUtc, ctx.nowUtc, ctx.intervalMinutes);

            info = new DailyRewardInfo(displayDay, Config.RewardsByDay,
                Config.GetRewardsForDay(displayDay) ?? Array.Empty<ICurrency>());
            return true;
        }

        public DailyRewardStatus GetRewardStatus()
        {
            if (!_playerProfileController.IsInitialized) return new DailyRewardStatus(false, false, TimeSpan.Zero, 0);

            var ctx = GetContext();
            var isMissed = IsStreakBroken(ctx.lastClaimUtc, ctx.nowUtc, ctx.intervalMinutes) && ctx.snapshot.LastClaimUtcTicks > 0;
            var timeUntilNext = TimeSpan.Zero;

            if (ctx.isClaimedInCurrentPeriod && ctx.lastClaimUtc != DateTime.MinValue)
            {
                var nextAvailableTime = ctx.lastClaimUtc.AddMinutes(ctx.intervalMinutes);
                timeUntilNext = Max(TimeSpan.Zero, nextAvailableTime - ctx.nowUtc);
            }

            return new DailyRewardStatus(!ctx.isClaimedInCurrentPeriod, isMissed, timeUntilNext, ctx.snapshot.CurrentDayIndex);
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;

        public bool IsCollectedDay(int dayIndex)
        {
            if (!_playerProfileController.IsInitialized)
                return false;
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot();
            if (snapshot?.ClaimedDaysIndexes == null || snapshot.ClaimedDaysIndexes.Count == 0)
                return false;
            return snapshot.ClaimedDaysIndexes.Contains(dayIndex);
        }


        public bool IsStreakBrokenAndNotReset()
        {
            if (!_playerProfileController.IsInitialized)
                return false;
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);
            var nowUtc = _bridgeService.GetServerTime().ToUniversalTime();
            var lastClaimUtc = snapshot.LastClaimUtcTicks > 0
                ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;
            var intervalMinutes = Config?.RewardIntervalMinutes ?? DailyRewardsSettingsConfiguration.DefaultRewardIntervalMinutes;
            return IsStreakBroken(lastClaimUtc, nowUtc, intervalMinutes) && !IsSnapshotReset(snapshot);
        }


        public bool TryGetResetSnapshotIfStreakBroken(out DailyRewardSnapshot resetSnapshot)
        {
            resetSnapshot = null;
            if (!_playerProfileController.IsInitialized)
                return false;
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);
            var nowUtc = _bridgeService.GetServerTime().ToUniversalTime();
            var lastClaimUtc = snapshot.LastClaimUtcTicks > 0
                ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;
            var intervalMinutes = Config?.RewardIntervalMinutes ?? DailyRewardsSettingsConfiguration.DefaultRewardIntervalMinutes;
            if (!IsStreakBroken(lastClaimUtc, nowUtc, intervalMinutes) || IsSnapshotReset(snapshot))
                return false;
            resetSnapshot = new DailyRewardSnapshot(1, snapshot.LastClaimUtcTicks, null);
            return true;
        }

        internal int CalculateNextDayIndex(int current, DateTime lastClaimUtc, DateTime nowUtc, int intervalMinutes)
        {
            if (IsStreakBroken(lastClaimUtc, nowUtc, intervalMinutes))
                return 1;

            return (current % DailyRewardsSettingsConfiguration.CycleLength) + 1;
        }

        internal (DailyRewardSnapshot snapshot, DateTime nowUtc, DateTime lastClaimUtc, bool isClaimedInCurrentPeriod, int intervalMinutes) GetContext()
        {
            var snapshot = _playerProfileController.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);

            var nowUtc = _bridgeService.GetServerTime().ToUniversalTime();
            var lastClaimUtc = snapshot.LastClaimUtcTicks > 0
                ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;

            var intervalMinutes = Config?.RewardIntervalMinutes ?? DailyRewardsSettingsConfiguration.DefaultRewardIntervalMinutes;
            var isClaimedInCurrentPeriod = lastClaimUtc != DateTime.MinValue &&
                (nowUtc - lastClaimUtc).TotalMinutes < intervalMinutes;

            if (IsStreakBroken(lastClaimUtc, nowUtc, intervalMinutes) && !IsSnapshotReset(snapshot))
            {
                var effectiveResetSnapshot = new DailyRewardSnapshot(1, snapshot.LastClaimUtcTicks, null);
                return (effectiveResetSnapshot, nowUtc, lastClaimUtc, isClaimedInCurrentPeriod, intervalMinutes);
            }

            return (snapshot, nowUtc, lastClaimUtc, isClaimedInCurrentPeriod, intervalMinutes);
        }

        private static bool IsStreakBroken(DateTime lastClaimUtc, DateTime nowUtc, int intervalMinutes)
        {
            if (lastClaimUtc == DateTime.MinValue) return true;
            double minutesPassed = (nowUtc - lastClaimUtc).TotalMinutes;
            double gracePeriodMinutes = intervalMinutes * DailyRewardsSettingsConfiguration.GracePeriodMultiplier;
            return minutesPassed >= gracePeriodMinutes;
        }

        private static bool IsSnapshotReset(DailyRewardSnapshot snapshot)
        {
            return snapshot.CurrentDayIndex == 1 && (snapshot.ClaimedDaysIndexes == null || snapshot.ClaimedDaysIndexes.Count == 0);
        }
    }
}
