using RSG;
using Services;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.AuthenticateSequence
{
    public class AuthenticatePlayerState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly BridgeService _bridgeService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;

        private IUIBlockRef _blockRef;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            if (!_bridgeService.ShouldAuthenticatePlayer)
            {
                LoggerService.LogWarning(this, $"Authentication is not required");
                FinishSequence();
                return;
            }

            if (_bridgeService.IsAuthenticated)
            {
                LoggerService.LogWarning(this, $"Player is already authenticated");
                FinishSequence();
                return;
            }

            PrepareEnvironment();
            AuthenticatePlayer()
                .ContinueWithResolved(() =>
                {
                    //TODO ADD LOGIC FOR MERGING PROFILES
                    RevertEnvironment();
                    NextState();
                })
                .CancelWith(this);
        }

        private IPromise AuthenticatePlayer()
        {
            return _bridgeService.AuthenticatePlayer();
        }
        
        private void NextState()
        {
            Sequence.ActivateState<OnAuthenticationCompleteState>();
        }

        private void FinishSequence()
        {
            Sequence.Finish();
        }
        
        private void PrepareEnvironment()
        {
            _blockRef = _uiScreenBlocker.Block(30f);
        }
        
        private void RevertEnvironment()
        {
            _blockRef?.Dispose();
        }
    }
}