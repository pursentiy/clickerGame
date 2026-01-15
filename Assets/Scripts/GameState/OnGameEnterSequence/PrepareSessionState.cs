using Extensions;
using Handlers;
using Handlers.UISystem;
using JetBrains.Annotations;
using Platform.Common.Utilities.StateMachine;
using RSG;
using Services;
using Storage;
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
        [Inject] private readonly GlobalSettingsService _globalSettingsService;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            _globalSettingsService.InitializeProfileSettings();

            SetupSounds();
            
            SetupProfile()
                .ContinueWithResolved(NextState)
                .CancelWith(this);
        }
        
        private void SetupSounds()
        {
            _soundHandler.SetMusicVolume(_globalSettingsService.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_globalSettingsService.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }
        
        private IPromise SetupProfile()
        {
            var setupPromise = new Promise();
            
            _playerRepositoryService.LoadPlayerSnapshot()
                .Then(OnPlayerSnapshotLoaded)
                .ContinueWithResolved(setupPromise.SafeResolve)
                .CancelWith(this);
            
            return setupPromise;

            IPromise OnPlayerSnapshotLoaded(ProfileSnapshot maybeSnapshot)
            {
                return maybeSnapshot == null ? StartNewProfileSession() : StartOldProfileSession(maybeSnapshot);
            }
        }
        
        private IPromise StartNewProfileSession()
        {
            var playerSnapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerService.Initialize(playerSnapshot);
            _playerRepositoryService.SavePlayerSnapshot(playerSnapshot);

            _playerProgressService.InitializeHandler(_levelsParamsStorageData.DefaultPacksParamsList);
            
            return Promise.Resolved();
        }

        private IPromise StartOldProfileSession(ProfileSnapshot profileSnapshot)
        {
            _playerService.Initialize(profileSnapshot);
            _playerProgressService.InitializeHandler(_levelsParamsStorageData.DefaultPacksParamsList);
            
            return Promise.Resolved();
        }

        private void NextState()
        {
            Sequence.ActivateState<ShowScreenOnGameFullyLoadedState>();
        }
    }
}