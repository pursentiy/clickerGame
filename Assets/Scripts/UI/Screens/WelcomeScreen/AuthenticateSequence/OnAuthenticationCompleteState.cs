using Services;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.AuthenticateSequence
{
    public class OnAuthenticationCompleteState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly BridgeService _bridgeService;
        [Inject] private readonly ReloadService _reloadService;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            LoggerService.LogDebug(this, 
                $"<color=green>[{GetType().Name}]</color> Player authorization status is {_bridgeService.IsAuthenticated}");
            
            _reloadService.SoftRestart();
            FinishSequence();
        }

        private void FinishSequence()
        {
            Sequence.Finish();
        }
    }
}