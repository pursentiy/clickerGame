using Controllers;
using Handlers;
using Handlers.UISystem;
using JetBrains.Annotations;
using Services;
using Services.Configuration;
using Services.CoroutineServices;
using Services.Player;
using Services.ScreenBlocker;
using Services.ScreenObserver;
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
            _applicationService.RegisterDisposableService<GameSoundManager>();
            _applicationService.RegisterDisposableService<GameInfoProvider>();
            _applicationService.RegisterDisposableService<UserSettingsService>();
            _applicationService.RegisterDisposableService<PlayerProfileManager>();
            _applicationService.RegisterDisposableService<AdsService>();
            _applicationService.RegisterDisposableService<ScreenObserverService>();
            _applicationService.RegisterDisposableService<UIScreenBlocker>();
            _applicationService.RegisterDisposableService<UIGlobalBlocker>();
            _applicationService.RegisterDisposableService<FlowScreenController>();
            _applicationService.RegisterDisposableService<FlowPopupController>();
            _applicationService.RegisterDisposableService<ScreenTransitionParticlesHandler>();
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