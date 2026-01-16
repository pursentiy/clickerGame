using Handlers;
using Handlers.UISystem;
using JetBrains.Annotations;
using RSG;
using Services;
using Services.Player;
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
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly LanguageConversionService _languageConversionService;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            
            //TODO CATCH ERROR RELOAD
            LoadOrInitProfile()
                .Then(SetupPlayer)
                .Catch(LoggerService.LogError)
                .CancelWith(this);
        }
        
        private IPromise<ProfileSnapshot> LoadOrInitProfile()
        {
           return _playerProfileManager.LoadProfile()
                .Then(OnPlayerSnapshotLoaded)
                .CancelWith(this);

            IPromise<ProfileSnapshot> OnPlayerSnapshotLoaded(ProfileSnapshot loadedProfileSnapshot)
            {
                return loadedProfileSnapshot == null ? BuildNewProfile() : Promise<ProfileSnapshot>.Resolved(loadedProfileSnapshot);
            }
        }
        
        private IPromise<ProfileSnapshot> BuildNewProfile()
        {
            var playerSnapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerProfileManager.Initialize(playerSnapshot);
            _playerProfileManager.SaveProfile();
            
            return Promise<ProfileSnapshot>.Resolved(playerSnapshot);
        }

        private void SetupPlayer(ProfileSnapshot profileSnapshot)
        {
            if (profileSnapshot == null)
            {
                LoggerService.LogError($"[{GetType().Name}]: {nameof(ProfileSnapshot)} is null");

                //TODO ADD MAX ATTEMPTS
                BuildNewProfile()
                    .Then(SetupPlayer)
                    .CancelWith(this);
            }
            
            _playerProfileManager.Initialize(profileSnapshot);
            
            _soundHandler.SetMusicVolume(_playerProfileManager.IsMusicOn);
            _soundHandler.SetSoundVolume(_playerProfileManager.IsSoundOn);
            _soundHandler.StartAmbience();
            _globalSettingsService.SetCurrentLanguage(_languageConversionService.GetLocale(_playerProfileManager.LanguageCode));

            NextState();
        }

        private void NextState()
        {
            Sequence.ActivateState<ShowScreenOnGameFullyLoadedState>();
        }
    }
}