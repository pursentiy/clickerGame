using Attributes;
using Common.Data.Info;
using Controllers;
using Extensions;
using Handlers.UISystem.Screens;
using Level.Widgets;
using Services;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly
{
    [AssetKey("UI Screens/PuzzleAssemblyScreenMediator")]
    public class PuzzleAssemblyScreenMediator : UIScreenBase<PuzzleAssemblyScreenView, PuzzleAssemblyScreenContext>
    {
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        
        public override void OnCreated()
        {
            base.OnCreated();
            
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.GoBackButton.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);

            InitializeWidgets();
        }

        private void InitializeWidgets()
        {
            View.LevelTimerWidget.Initialize();
            View.StarsProgressWidget.Initialize(Context.LevelParamsSnapshot.LevelBeatingTimeInfo);
            View.LevelSessionHandler.Initialize(Context.LevelParamsSnapshot, Context.PackId);
        }
        
        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void OnGoBackButtonClicked()
        {
            var packInfo = _progressProvider.GetPackInfo(Context.PackId);
            if (packInfo == null)
            {
                LoggerService.LogWarning(this, $"{nameof(PackInfo)} is null for PackId {Context.PackId}. Returning to Welcome Screen");
                _flowScreenController.GoToWelcomeScreen();
                return;
            }
            
            View.LevelSessionHandler.OnScreenLeave();
            _flowScreenController.GoToChooseLevelScreen(packInfo);
        }
    }
}