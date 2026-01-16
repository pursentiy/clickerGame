using Handlers.UISystem;
using JetBrains.Annotations;
using Platform.Common.Utilities.StateMachine;
using Services;
using Services.CoroutineServices;
using Services.Player;
using Utilities.StateMachine;
using Zenject;

namespace GameState.OnGameEnterSequence
{
    [UsedImplicitly]
    public class StartServicesState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _applicationService.RegisterDisposableService(_uiManager);
            _applicationService.RegisterDisposableService<TimeService>();
            _applicationService.RegisterDisposableService<PersistentCoroutinesService>();
            _applicationService.RegisterDisposableService<CoroutineService>();
            _applicationService.RegisterDisposableService<GlobalSettingsService>();
            _applicationService.RegisterDisposableService<GameConfigurationProvider>();
            _applicationService.RegisterDisposableService<GameManager>();
            _applicationService.SetApplicationInitialized();
            
            LoggerService.LogWarning($"{nameof(StartServicesState)}: Disposable Services  registered");

            NextState();
        }

        private void NextState()
        {
            Sequence.ActivateState<PrepareSessionState>();
        }
    }
}