#if UNITY_EDITOR
using System.Linq;
using Extensions;
using Handlers.UISystem;
using Services.Player;
using UI.Popups.CompleteLevelInfoPopup;
using UI.Popups.UniversalPopup;
using UnityEngine;
using Zenject;

namespace Services.Cheats
{
    public class CheatService
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ReloadService _reloadService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;

        private const string StarsCountKey = "CheatService_StarsCount";
        private const string EarnedStarsKey = "CheatService_EarnedStars";
        private const string TimeScaleKey = "CheatService_TimeScale";
        private const string UpdateProfileKey = "CheatService_UpdateProfile";
        private const string LevelStatusKey = "CheatService_LevelStatus";

        public int StarsCount
        {
            get => PlayerPrefs.GetInt(StarsCountKey, 5);
            set => PlayerPrefs.SetInt(StarsCountKey, value);
        }

        public int EarnedStars
        {
            get => PlayerPrefs.GetInt(EarnedStarsKey, 5);
            set => PlayerPrefs.SetInt(EarnedStarsKey, value);
        }

        public float GameTimeScale
        {
            get => PlayerPrefs.GetFloat(TimeScaleKey, 1);
            set => PlayerPrefs.SetFloat(TimeScaleKey, value);
        }

        public bool UpdateProfileValues
        {
            get => PlayerPrefs.GetInt(UpdateProfileKey, 0) == 1;
            set => PlayerPrefs.SetInt(UpdateProfileKey, value ? 1 : 0);
        }

        public CompletedLevelStatus LevelStatus
        {
            get => (CompletedLevelStatus)PlayerPrefs.GetInt(LevelStatusKey, (int)CompletedLevelStatus.InitialCompletion);
            set => PlayerPrefs.SetInt(LevelStatusKey, (int)value);
        }

        public void ShowCompleteLevelInfoPopupMediator()
        {
            if (!_uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>().IsNullOrEmpty())
                return;

            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(
                new CompleteLevelInfoPopupContext(EarnedStars, StarsCount, 1, 10, LevelStatus));
        }
        
        public void VisualizeStarsFlightInCompleteLevelPopup()
        {
            var popups = _uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>();
            if (popups.IsNullOrEmpty())
                return;
            
            var popup = popups.First();
            if (popup == null)
                return;
            
            popup.PlayStarsAnimation(StarsCount, UpdateProfileValues);
        }
        
        public void ResetProgress()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
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

        public void AdsCheatShowSuccess() => _adsService.CheatShowSuccess();
        public void AdsCheatShowException() => _adsService.CheatShowException();
        public void AdsCheatShowTimeout() => _adsService.CheatShowTimeout();

        public void AddStars()
        {
            if (StarsCount > 0)
            {
                _playerCurrencyService.TryAddStars(StarsCount);
            }
            else if (StarsCount < 0)
            {
                _playerCurrencyService.TrySpendStars(-StarsCount);
            }
        }

        public void SetGameTimeScale()
        {
            Time.timeScale = GameTimeScale;
        }
        
        public void SaveAll() => PlayerPrefs.Save();
    }
}
#endif