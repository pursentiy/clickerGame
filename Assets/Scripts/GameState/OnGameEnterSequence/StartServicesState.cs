using Handlers.UISystem;
using JetBrains.Annotations;
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
            _applicationService.RegisterDisposableService<GameSettingsManager>();
            _applicationService.RegisterDisposableService<GameConfigurationProvider>();
            _applicationService.RegisterDisposableService<GameParamsManager>();
            _applicationService.RegisterDisposableService<PlayerProfileManager>();
            _applicationService.RegisterDisposableService<AdsService>();
            _applicationService.SetApplicationInitialized();
            
            LoggerService.LogDebug($"{nameof(StartServicesState)}: Disposable Services  registered");

            NextState();
        }

        private void NextState()
        {
            Sequence.ActivateState<PrepareSessionState>();
        }
    }
}