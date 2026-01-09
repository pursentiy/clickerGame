using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Handlers;
using Handlers.UISystem;
using Installers;
using Services;
using Storage;
using Storage.Levels;
using Zenject;
using ScreenHandler = Handlers.ScreenHandler;

namespace GameState
{
    public class OnGameEnterState : InjectableMonoBehaviour
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly GlobalSettingsService _globalSettingsService;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorage;
        [Inject] private readonly LocalizationService _localizationService;

        protected override void Awake()
        {
            DOTween.Init(true, true, LogBehaviour.Default);
            
            _applicationService.RegisterDisposableService(_uiManager);

            LoggerService.LogDebugEditor($"[{nameof(OnGameEnterState)}] Awake finished");
        }

        private IEnumerator Start()
        {
            LoggerService.LogDebugEditor($"[{nameof(OnGameEnterState)}]  Start: Waiting for Localization...");
            
            yield return _localizationService.InitializeRoutine();

            if (!_localizationService.IsInitialized)
            {
                LoggerService.LogDebugEditor($"[{nameof(OnGameEnterState)}]  Localization failed to initialize. Stopping.");
                yield break;
            }

            LoggerService.LogDebugEditor($"[{nameof(OnGameEnterState)}]  Proceeding with UI Setup...");
            
            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            _globalSettingsService.InitializeProfileSettings();

            if (_playerRepositoryService.HasProfile)
            {
                StartOldProfileSession();
            }
            else
            {
                StartNewProfileSession();
            }

            SetupSounds();
            _screenHandler.InitializeBackground();
            _screenHandler.ShowWelcomeScreen(true);

            LoggerService.LogDebugEditor($"[{nameof(OnGameEnterState)}] Successfully entered game.");
        }

        private void StartNewProfileSession()
        {
            var playerSnapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerService.Initialize(playerSnapshot);
            _playerRepositoryService.SavePlayerSnapshot(playerSnapshot);
                
            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private void StartOldProfileSession()
        {
            _playerService.Initialize(_playerRepositoryService.LoadPlayerSnapshot());
            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private List<PackParamsData> GetNewPacks(List<PackParamsData> savedDataProgress)
        {
            var diff = _levelsParamsStorage.DefaultPacksParamsList.Count - savedDataProgress.Count;
            var newPacks = new List<PackParamsData>();
            
            for (var index = savedDataProgress.Count; index <= diff; index++)
            {
                newPacks.Add(_levelsParamsStorage.DefaultPacksParamsList[index]);
            }

            return newPacks;
        }

        private void SetupSounds()
        {
            _soundHandler.SetMusicVolume(_globalSettingsService.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_globalSettingsService.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }
    }
}