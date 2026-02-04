using Handlers.UISystem;
using JetBrains.Annotations;
using Services;
using UI.Screens.WelcomeScreen;
using Utilities.StateMachine;
using Zenject;

namespace GameState.OnGameEnterSequence
{
    [UsedImplicitly]
    public class ShowScreenOnGameFullyLoadedState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly BridgeService _bridgeService;
        [Inject] private readonly UIManager _uiManager;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            _bridgeService.SetGameReady();
            _uiManager.ScreensHandler.PushFirstScreen<WelcomeScreenMediator>()
                .Then(() =>
                {
                    _adsService.ShowBanner();
                    NextState();
                });
        }
        
        private void NextState()
        {
            Sequence.ActivateState<GameIdleState>();
        }
    }
}