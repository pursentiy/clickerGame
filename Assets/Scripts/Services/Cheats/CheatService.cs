using System.Linq;
using Extensions;
using Handlers.UISystem;
using Popup.CompleteLevelInfoPopup;
using Zenject;

namespace Services.Cheats
{
    public class CheatService
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly UIManager _uiManager;

        public int StarsCount { get; set; } = 5;
        public bool UpdateProfileValues { get; set; }
        
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
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}