using Attributes;
using Controllers;
using Handlers.UISystem.Screens;
using Zenject;

namespace UI.Screens.PuzzleAssembly
{
    [AssetKey("UI Screens/PuzzleAssemblyScreenMediator")]
    public class PuzzleAssemblyScreenMediator : UIScreenBase<PuzzleAssemblyScreenView, PuzzleAssemblyScreenContext>
    {
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        
        public override void OnCreated()
        {
            base.OnCreated();

        }
        
        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }
    }
}