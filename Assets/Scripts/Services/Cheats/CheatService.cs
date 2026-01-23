#if UNITY_EDITOR
using System.Linq;
using Extensions;
using Handlers.UISystem;
using Popup.CompleteLevelInfoPopup;
using Popup.Universal;
using Services.Player;
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

        public int StarsCount { get; set; } = 5;
        public int GameTimeScale { get; set; } = 10;
        public bool UpdateProfileValues { get; set; }

        public void ShowCompleteLevelInfoPopupMediator()
        {
            if (!_uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>().IsNullOrEmpty())
                return;

            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(
                new CompleteLevelInfoPopupContext(3, 10));
        }
        
        public void VisualizeStarsFlightInCompleteLevelPopup()
        {
            var popups = _uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>();
            if (popups.IsNullOrEmpty())
                return;
            
            var popup = popups.First();
            if (popup == null)
                return;
            
            popup.PlayStarsAnimation(StarsCount,  UpdateProfileValues);
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

        public void AdsCheatShowSuccess()
        {
            _adsService.CheatShowSuccess();
        }
        
        public void AdsCheatShowException()
        {
            _adsService.CheatShowException();
        }
        
        public void AdsCheatShowTimeout()
        {
            _adsService.CheatShowTimeout();
        }

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
    }
}
#endif