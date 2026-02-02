using Attributes;
using Handlers.UISystem.Screens;

namespace UI.Screens.PuzzleAssemblyScreen
{
    [AssetKey("UI Screens/PuzzleAssemblyScreenMediator")]
    public class PuzzleAssemblyScreenMediator : UIScreenBase<PuzzleAssemblyScreenView, PuzzleAssemblyScreenContext>
    {
        public override void OnCreated()
        {
            base.OnCreated();

        }
    }
}