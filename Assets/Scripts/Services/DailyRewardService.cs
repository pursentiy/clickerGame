using System;
using System.Collections.Generic;
using Common.Currency;
using Configurations.DailyReward;
using Services.Base;
using Services.Configuration;
using Services.Player;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    /// <summary>
    /// Manages daily login reward progression and claiming.
    /// </summary>
    public class DailyRewardService : DisposableService
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly GameConfigurationProvider _configurationProvider;
        [Inject] private readonly BridgeService _bridgeService;

        private DailyRewardConfiguration _config;

        private DailyRewardConfiguration Config =>
            _config ??= _configurationProvider.GetConfig<DailyRewardConfiguration>();

        public bool TryGetTodayRewardPreview(out DailyRewardInfo rewardInfo)
        {
            rewardInfo = default;

            if (!_playerProfileManager.IsInitialized)
                return false;

            var config = Config;
            if (config == null)
                return false;

            var snapshot = _playerProfileManager.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);

            var today = _bridgeService.GetServerTime().Date;
            var lastClaimDate = snapshot.LastClaimUtcTicks > 0
                ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc).Date
                : DateTime.MinValue.Date;

            // Already claimed today
            if (lastClaimDate == today)
                return false;

            var nextDayIndex = CalculateNextDayIndex(snapshot.CurrentDayIndex, lastClaimDate, today);
            var rewardsForDay = config.GetRewardsForDay(nextDayIndex);

            if (rewardsForDay == null || rewardsForDay.Count == 0)
                return false;

            rewardInfo = new DailyRewardInfo(nextDayIndex, config.RewardsByDay, rewardsForDay);
            return true;
        }

        public DailyRewardStatus GetRewardStatus()
        {
            if (!_playerProfileManager.IsInitialized)
                return new DailyRewardStatus(false, false, TimeSpan.Zero, 0);

            var snapshot = _playerProfileManager.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);
            var today = _bridgeService.GetServerTime().Date;
            var lastClaimDate = snapshot.LastClaimUtcTicks > 0
                ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc).Date
                : DateTime.MinValue.Date;

            var isAvailable = lastClaimDate != today;
            var isMissed = lastClaimDate < today.AddDays(-1) && snapshot.LastClaimUtcTicks > 0;

            TimeSpan timeUntilNext;
            if (isAvailable)
            {
                timeUntilNext = TimeSpan.Zero;
            }
            else
            {
                var nextDay = today.AddDays(1);
                var now = _bridgeService.GetServerTime();
                timeUntilNext = nextDay - now;
            }

            return new DailyRewardStatus(isAvailable, isMissed, timeUntilNext, snapshot.CurrentDayIndex);
        }

        public bool TryClaimTodayReward(out DailyRewardInfo claimedRewardInfo)
        {
            claimedRewardInfo = default;

            if (!_playerProfileManager.IsInitialized)
                return false;

            var config = Config;
            if (config == null)
                return false;

            var currentSnapshot = _playerProfileManager.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);

            var today = _bridgeService.GetServerTime().Date;
            var lastClaimDate = currentSnapshot.LastClaimUtcTicks > 0
                ? new DateTime(currentSnapshot.LastClaimUtcTicks, DateTimeKind.Utc).Date
                : DateTime.MinValue.Date;

            // Already claimed today
            if (lastClaimDate == today)
                return false;

            var nextDayIndex = CalculateNextDayIndex(currentSnapshot.CurrentDayIndex, lastClaimDate, today);
            var rewardsForDay = config.GetRewardsForDay(nextDayIndex);
            if (rewardsForDay == null || rewardsForDay.Count == 0)
                return false;

            // Currently only Stars rewards are applied to the player's balance.
            var totalStars = 0;
            foreach (var currency in rewardsForDay)
            {
                if (currency is Stars stars)
                {
                    totalStars += stars.Value;
                }
            }

            if (totalStars > 0)
            {
                var starsReward = new Stars(totalStars);
                if (!_playerCurrencyService.TryAddStars(starsReward, CurrencyChangeMode.Instant))
                    return false;
            }
            else
            {
                return false;
            }

            currentSnapshot.CurrentDayIndex = nextDayIndex;
            currentSnapshot.LastClaimUtcTicks = today.Ticks;

            _playerProfileManager.UpdateDailyRewardAndSave(currentSnapshot, SavePriority.ImmediateSave);

            claimedRewardInfo = new DailyRewardInfo(nextDayIndex, config.RewardsByDay, rewardsForDay);
            return true;
        }

        private int CalculateNextDayIndex(int currentDayIndex, DateTime lastClaimDate, DateTime today)
        {
            // If reward was missed (burned), reset to day 1
            if (lastClaimDate < today.AddDays(-1) && lastClaimDate != DateTime.MinValue.Date)
            {
                return 1;
            }

            if (lastClaimDate == today.AddDays(-1) && currentDayIndex > 0)
            {
                // Continue streak within 7-day cycle
                var next = currentDayIndex + 1;
                if (next > DailyRewardConfiguration.CycleLength)
                    next = 1;
                return next;
            }

            // Reset streak
            return 1;
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnDisposing()
        {
        }
    }

    public readonly struct DailyRewardInfo
    {
        public readonly int DayIndex;
        public readonly IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay;
        public readonly IList<ICurrency> EarnedDailyReward;

        public DailyRewardInfo(
            int dayIndex,
            IReadOnlyDictionary<int, IList<ICurrency>> rewardsByDay,
            IList<ICurrency> earnedDailyReward)
        {
            DayIndex = dayIndex;
            RewardsByDay = rewardsByDay;
            EarnedDailyReward = earnedDailyReward;
        }
    }

    public readonly struct DailyRewardStatus
    {
        public readonly bool IsAvailable;
        public readonly bool IsMissed;
        public readonly TimeSpan TimeUntilNext;
        public readonly int CurrentDayIndex;

        public DailyRewardStatus(bool isAvailable, bool isMissed, TimeSpan timeUntilNext, int currentDayIndex)
        {
            IsAvailable = isAvailable;
            IsMissed = isMissed;
            TimeUntilNext = timeUntilNext;
            CurrentDayIndex = currentDayIndex;
        }
    }
}

