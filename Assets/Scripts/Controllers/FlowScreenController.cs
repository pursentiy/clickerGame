using Common.Data.Info;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Handlers.UISystem.Screens.Transitions;
using RSG;
using Storage.Snapshots.LevelParams;
using UI.Screens.ChooseLevel;
using UI.Screens.ChoosePack;
using UI.Screens.PuzzleAssembly;
using UI.Screens.WelcomeScreen;
using Utilities.Disposable;
using Zenject;

namespace Controllers
{
    public sealed class FlowScreenController : FlowControllerBase
    {
        [Inject] private readonly UIManager _uiManager;

        public MediatorFlowInfo GoToChoosePackScreen()
        {
            var screenPromise = GoToScreenInternal(new FadeScreenTransition(typeof(ChoosePackScreenMediator), null));
            return ToFlowInfo(screenPromise);
        }
        
        public MediatorFlowInfo GoToChooseLevelScreen(PackInfo packInfo)
        {
            var context = new ChooseLevelScreenContext(packInfo);
            var screenPromise = GoToScreenInternal(new FadeScreenTransition(typeof(ChooseLevelScreenMediator), context));
            return ToFlowInfo(screenPromise);
        }
        public MediatorFlowInfo GoToPuzzleAssemblyScreen(LevelParamsSnapshot levelParamsSnapshot, int packId)
        {
            var context = new PuzzleAssemblyScreenContext(levelParamsSnapshot, packId);
            var screenPromise = GoToScreenInternal(new FadeScreenTransition(typeof(PuzzleAssemblyScreenMediator), context));
            return ToFlowInfo(screenPromise);
        }
        
        public MediatorFlowInfo GoToWelcomeScreen()
        {
            var screenPromise = GoToScreenInternal(new FadeScreenTransition(typeof(WelcomeScreenMediator), null));
            return ToFlowInfo(screenPromise);
        }
        
        private IPromise<UIScreenBase> GoToScreenInternal(UIScreenTransition transition)
        {
            return _uiManager.ScreensHandler.PushScreenClean(transition).CancelWith(this);
        }

        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            
        }
    }
}