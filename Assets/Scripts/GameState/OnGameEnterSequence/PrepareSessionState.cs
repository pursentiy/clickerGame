using Handlers.UISystem;
using JetBrains.Annotations;
using RSG;
using Services;
using Services.Player;
using Storage.Snapshots;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace GameState.OnGameEnterSequence
{
    [UsedImplicitly]
    public class PrepareSessionState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerProfileController _playerProfileController;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            //TODO CATCH ERROR RELOAD
            _playerProfileController.LoadProfile()
                .Then(SetupPlayer)
                .Then(SetupScreen)
                .Then(NextState)
                .Catch(LoggerService.LogError)
                .CancelWith(this);
        }

        private IPromise SetupPlayer(ProfileSnapshot loadedProfileSnapshot)
        {
            var requireProfileSaving = loadedProfileSnapshot == null;
            var finalProfileSnapshot = loadedProfileSnapshot ?? _profileBuilderService.BuildNewProfileSnapshot();

            _playerProfileController.Initialize(finalProfileSnapshot);
            if (requireProfileSaving)
                _playerProfileController.SaveProfile(SavePriority.ImmediateSave);

            return Promise.Resolved();
        }

        private IPromise SetupScreen()
        {
            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            
            return Promise.Resolved();
        }

        private void NextState()
        {
            Sequence.ActivateState<ShowScreenOnGameFullyLoadedState>();
        }
    }
}