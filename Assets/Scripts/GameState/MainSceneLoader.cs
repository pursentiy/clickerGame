using GameState.OnGameEnterSequence;
using Handlers.UISystem;
using Installers;
using Platform.Common.Utilities.StateMachine;
using Services;
using Services.CoroutineServices;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace GameState
{
    public class MainSceneLoader : InjectableMonoBehaviour
    {
        [Inject] private CoroutineService _coroutineService;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;
        
        private IStateSequence _machine;

        protected override void Awake()
        {
            base.Awake();
            
            StartStateMachine();
        }

        private void StartStateMachine()
        {
            _machine = StateMachine
                .CreateMachine(null)
                .StartSequence<StartServicesState>()
                .FinishWith(this);
        }
    }
}