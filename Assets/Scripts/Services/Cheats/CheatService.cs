#if UNITY_EDITOR
using System.Linq;
using Extensions;
using Handlers.UISystem;
using Popup.CompleteLevelInfoPopup;
using Popup.Universal;
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

        public int StarsCount { get; set; } = 5;
        public bool UpdateProfileValues { get; set; }

        public void ShowCompleteLevelInfoPopupMediator()
        {
            if (!_uiManager.PopupsHandler.GetShownPopups<CompleteLevelInfoPopupMediator>().IsNullOrEmpty())
                return;

            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(
                new CompleteLevelInfoPopupContext(3, 0, 10, null));
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
                _localizationService.GetCommonValue(LocalizationExtensions.ChangeLanguageNotifyKey),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.CancelKey), null, UniversalPopupButtonStyle.Red),
                    new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.ChangeKey), _reloadService.SoftRestart)
                }, _localizationService.GetCommonValue(LocalizationExtensions.ChangeLanguageTitle));
            
            
            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }
    }
}
#endif