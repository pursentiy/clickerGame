using Handlers;
using JetBrains.Annotations;
using Platform.Common.Utilities.StateMachine;
using Services;
using Utilities.StateMachine;
using Zenject;

namespace GameState.OnGameEnterSequence
{
    [UsedImplicitly]
    public class ShowScreenOnGameFullyLoadedState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly AdsService _adsService;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            _adsService.ShowBanner();
            _screenHandler.ShowWelcomeScreen(true);

            NextState();
        }
        
        private void NextState()
        {
            Sequence.ActivateState<GameIdleState>();
        }
    }
}