#if UNITY_EDITOR
using System;
using System.Linq;
using Common.Currency;
using Extensions;
using Handlers.UISystem;
using Services.Player;
using Storage.Snapshots;
using UI.Popups.CompleteLevelInfoPopup;
using UI.Popups.DailyRewardPopup;
using UI.Popups.UniversalPopup;
using UnityEngine;
using Zenject;

namespace Services.Cheats
{
    public class CheatService
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly PlayerProfileController _playerProfileController;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ReloadService _reloadService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        [Inject] private readonly BridgeService _bridgeService;

        private const string StarsCountKey = "CheatService_StarsCount";
        private const string EarnedStarsKey = "CheatService_EarnedStars";
        private const string TimeScaleKey = "CheatService_TimeScale";
        private const string UpdateProfileKey = "CheatService_UpdateProfile";
        private const string LevelStatusKey = "CheatService_LevelStatus";

        [CheatGroup("Settings")]
        public int StarsCount
        {
            get => PlayerPrefs.GetInt(StarsCountKey, 5);
            set => PlayerPrefs.SetInt(StarsCountKey, value);
        }

        [CheatGroup("Settings")]
        public int EarnedStars
        {
            get => PlayerPrefs.GetInt(EarnedStarsKey, 5);
            set => PlayerPrefs.SetInt(EarnedStarsKey, value);
        }

        [CheatGroup("Settings")]
        public float GameTimeScale
        {
            get => PlayerPrefs.GetFloat(TimeScaleKey, 1);
            set => PlayerPrefs.SetFloat(TimeScaleKey, value);
        }

        [CheatGroup("Settings")]
        public bool UpdateProfileValues
        {
            get => PlayerPrefs.GetInt(UpdateProfileKey, 0) == 1;
            set => PlayerPrefs.SetInt(UpdateProfileKey, value ? 1 : 0);
        }

        [CheatGroup("Settings")]
        public CompletedLevelStatus LevelStatus
        {
            get => (CompletedLevelStatus)PlayerPrefs.GetInt(LevelStatusKey, (int)CompletedLevelStatus.InitialCompletion);
            set => PlayerPrefs.SetInt(LevelStatusKey, (int)value);
        }

        [CheatGroup("Complete Level Popup")]
        public void ShowCompleteLevelInfoPopupMediator()
        {
            if (!_uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>().IsNullOrEmpty())
                return;

            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(
                new CompleteLevelInfoPopupContext(EarnedStars, StarsCount, 1, 10, LevelStatus));
        }
        
        [CheatGroup("Complete Level Popup")]
        public void VisualizeStarsFlightInCompleteLevelPopup()
        {
            var popups = _uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>();
            if (popups.IsNullOrEmpty())
                return;
            
            var popup = popups.First();
            if (popup == null)
                return;
            
            popup.PlayStarsAnimation(StarsCount);
        }

        [CheatGroup("Daily Rewards")]
        /// <summary>
        /// Plays the daily reward claim/receiving animation. Daily reward popup must be open.
        /// </summary>
        public void PlayDailyRewardReceivingAnimation()
        {
            var popups = _uiManager.PopupsHandler.GetShownPopups<DailyRewardPopupMediator>();
            if (popups.IsNullOrEmpty())
                return;

            var popup = popups.First();
            if (popup != null)
                popup.PlayClaimReceivingAnimation();
        }
        
        [CheatGroup("Progress")]
        public void ResetProgress()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
        [CheatGroup("Progress")]
        public void ShowReloadPopup()
        {
            if (!_uiManager.PopupsHandler.GetShownPopups<UniversalPopupMediator>().IsNullOrEmpty())
                return;
            
            var context = new UniversalPopupContext(
                _localizationService.GetValue(LocalizationExtensions.ChangeLanguageNotifyKey),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetValue(LocalizationExtensions.CancelKey), null, UniversalPopupButtonStyle.Red),
                    new UniversalPopupButtonAction(_localizationService.GetValue(LocalizationExtensions.ChangeKey), _reloadService.SoftRestart)
                }, _localizationService.GetValue(LocalizationExtensions.ChangeLanguageTitle));
            
            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }

        [CheatGroup("Ads")]
        public void AdsCheatShowSuccess() => _adsService.CheatShowSuccess();
        [CheatGroup("Ads")]
        public void AdsCheatShowException() => _adsService.CheatShowException();
        [CheatGroup("Ads")]
        public void AdsCheatShowTimeout() => _adsService.CheatShowTimeout();

        [CheatGroup("Currency")]
        public void AddStars()
        {
            if (StarsCount > 0)
            {
                _playerCurrencyManager.TryAddCurrency(new Stars(StarsCount));
            }
            else if (StarsCount < 0)
            {
                _playerCurrencyManager.TrySpendCurrency(new Stars(StarsCount));
            }
        }

        [CheatGroup("Time")]
        public void SetGameTimeScale()
        {
            Time.timeScale = GameTimeScale;
        }

        [CheatGroup("Daily Rewards")]
        /// <summary>
        /// Resets daily rewards to day 1; reward can be collected again (day 1).
        /// </summary>
        public void ResetDailyRewardsProgress()
        {
            if (!_playerProfileController.IsInitialized)
                return;

            var snapshot = new DailyRewardSnapshot(1, 0);
            _playerProfileController.UpdateDailyRewardAndSave(snapshot, SavePriority.ImmediateSave);
        }

        [CheatGroup("Daily Rewards")]
        /// <summary>
        /// Skips daily reward so the next day's reward can be collected again (last claim set to yesterday).
        /// </summary>
        public void SkipDailyRewardToNextDay()
        {
            if (!_playerProfileController.IsInitialized)
                return;

            var current = _playerProfileController.TryGetDailyRewardSnapshot();
            var currentDay = current?.CurrentDayIndex ?? 1;
            var yesterdayUtc = DateTime.UtcNow.Date.AddDays(-1).Ticks;

            var snapshot = new DailyRewardSnapshot(currentDay, yesterdayUtc);
            _playerProfileController.UpdateDailyRewardAndSave(snapshot, SavePriority.ImmediateSave);
        }

        [CheatGroup("Daily Rewards")]
        /// <summary>
        /// Sets last claim time so the next daily reward will be available in 15 seconds.
        /// </summary>
        public void SkipTheTimeToTheNextDailyRewards()
        {
            if (!_playerProfileController.IsInitialized)
                return;

            var current = _playerProfileController.TryGetDailyRewardSnapshot();
            var currentDay = current?.CurrentDayIndex ?? 1;
            
            // Set LastClaimUtcTicks to (now - 1 day + 15 seconds) so that in 15 seconds,
            // lastClaimDate will still be yesterday, making the reward available
            var now = _bridgeService.GetServerTime();
            var targetTime = now - TimeSpan.FromDays(1) + TimeSpan.FromSeconds(15);
            var lastClaimTicks = targetTime.Ticks;

            var snapshot = new DailyRewardSnapshot(currentDay, lastClaimTicks);
            _playerProfileController.UpdateDailyRewardAndSave(snapshot, SavePriority.ImmediateSave);
        }

        [CheatGroup("Other")]
        public void SaveAll() => PlayerPrefs.Save();
    }
}
#endif