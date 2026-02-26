using System.Collections.Generic;
using Services.Player;
using Storage.Snapshots;
using Zenject;

namespace Services.DailyReward
{
    /// <summary>
    /// Updates and persists daily reward state (claim, save).
    /// </summary>
    public sealed class DailyRewardController
    {
        [Inject] private readonly PlayerProfileController _playerProfileController;
        [Inject] private readonly DailyRewardsInfoProvider _dailyRewardsInfoProvider;
        [Inject] private readonly BridgeService _bridgeService;

        /// <summary>
        /// If streak is broken and snapshot is not already reset, persists the reset snapshot. Returns true if reset was saved.
        /// </summary>
        public bool TryResetDailyRewardsStreak()
        {
            if (!_playerProfileController.IsInitialized)
                return false;
            if (!_dailyRewardsInfoProvider.TryGetResetSnapshotIfStreakBroken(out var resetSnapshot))
                return false;
            _playerProfileController.UpdateDailyRewardAndSave(resetSnapshot, SavePriority.ImmediateSave);
            return true;
        }

        /// <summary>
        /// Creates and saves a reset snapshot (day 1, empty claimed list). Use when TryResetDailyRewardsStreak failed.
        /// </summary>
        public void SaveResetSnapshot()
        {
            if (!_playerProfileController.IsInitialized)
                return;
            var current = _playerProfileController.TryGetDailyRewardSnapshot();
            var resetSnapshot = new DailyRewardSnapshot(1, current?.LastClaimUtcTicks ?? 0, null);
            _playerProfileController.UpdateDailyRewardAndSave(resetSnapshot, SavePriority.Default);
        }

        /// <summary>
        /// Claims today's reward: updates snapshot (day index and last claim time) and saves profile.
        /// Returns false if there is nothing to claim (e.g. already claimed or not initialized).
        /// </summary>
        public bool TryClaimTodayReward()
        {
            if (!_dailyRewardsInfoProvider.TryGetTodayRewardPreview(out var info))
                return false;

            if (!_playerProfileController.IsInitialized)
                return false;

            var current = _playerProfileController.TryGetDailyRewardSnapshot();
            var claimedList = current?.ClaimedDaysIndexes != null && current.ClaimedDaysIndexes.Count > 0
                ? new List<int>(current.ClaimedDaysIndexes)
                : new List<int>();
            if (info.DayIndex == 1)
                claimedList = new List<int> { 1 };
            else if (!claimedList.Contains(info.DayIndex))
                claimedList.Add(info.DayIndex);

            var newSnapshot = new DailyRewardSnapshot(info.DayIndex, _bridgeService.GetServerTime().ToUniversalTime().Ticks, claimedList);
            _playerProfileController.UpdateDailyRewardAndSave(newSnapshot, SavePriority.ImmediateSave);
            return true;
        }
    }
}
