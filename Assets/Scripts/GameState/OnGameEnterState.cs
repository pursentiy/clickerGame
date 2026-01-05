using System.Collections.Generic;
using Handlers;
using Handlers.UISystem;
using Installers;
using Services;
using Storage;
using Storage.Levels;
using Storage.Levels.Params;
using Zenject;
using ScreenHandler = Handlers.ScreenHandler;

namespace GameState
{
    public class OnGameEnterState : InjectableMonoBehaviour
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private ProfileBuilderService _profileBuilderService;
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private PlayerService _playerService;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private GlobalSettingsService _globalSettingsService;
        [Inject] private LevelsParamsStorageData _levelsParamsStorage;

        protected override void Awake()
        {
            _applicationService.RegisterDisposableService(_uiManager);
            
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

        private void Start()
        {
            _screenHandler.ShowWelcomeScreen(true);

            SetupSounds();
        }

        private void SetupSounds()
        {
            _soundHandler.SetMusicVolume(_globalSettingsService.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_globalSettingsService.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }

        private bool IsNewPacksAdded(List<PackParams> savedDataProgress)
        {
            return _levelsParamsStorage.DefaultPacksParamsList.Count > savedDataProgress.Count;
        }
    }
}