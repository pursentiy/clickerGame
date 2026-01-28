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
        [Inject] private readonly UIManager _uiManager;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            _uiManager.ScreensHandler.PushFirstScreen<WelcomeScreenScreenMediator>()
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