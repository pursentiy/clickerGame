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

            var reloadButton = new UniversalPopupButtonAction("Релоад", _reloadService.SoftRestart, UniversalPopupButtonStyle.Common);
            var cancelButton = new UniversalPopupButtonAction("Отмена", null, UniversalPopupButtonStyle.Red);
            var context = new UniversalPopupContext(
                "Если вы поменеяете язык, то игра перезагрузиться. Вы точно хотите поменять язык?",
                new[] { cancelButton, reloadButton }, "Смена языка");
            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }
    }
}
#endif