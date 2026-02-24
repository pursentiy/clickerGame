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
        /// Claims today's reward: updates snapshot (day index and last claim time) and saves profile.
        /// Returns false if there is nothing to claim (e.g. already claimed or not initialized).
        /// </summary>
        public bool TryClaimTodayReward()
        {
            if (!_dailyRewardsInfoProvider.TryGetTodayRewardPreview(out var info))
                return false;

            if (!_playerProfileController.IsInitialized)
                return false;

            var newSnapshot = new DailyRewardSnapshot(info.DayIndex, _bridgeService.GetServerTime().Date.Ticks);
            _playerProfileController.UpdateDailyRewardAndSave(newSnapshot, SavePriority.ImmediateSave);
            return true;
        }
    }
}
