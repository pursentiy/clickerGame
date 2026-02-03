using Attributes;
using Controllers;
using Extensions;
using Handlers;
using Handlers.UISystem.Screens;
using Level.Widgets;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly
{
    [AssetKey("UI Screens/PuzzleAssemblyScreenMediator")]
    public class PuzzleAssemblyScreenMediator : UIScreenBase<PuzzleAssemblyScreenView, PuzzleAssemblyScreenContext>
    {
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly LevelParamsHandler _levelParamsHandler;
        
        public override void OnCreated()
        {
            base.OnCreated();
            
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);

            InitializeWidgets();
        }

        private void InitializeWidgets()
        {
            View.LevelTimerWidget.Initialize();
            View.StarsProgressWidget.Initialize(Context.LevelParamsSnapshot.LevelBeatingTimeInfo);
        }
        
        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        
    }
}