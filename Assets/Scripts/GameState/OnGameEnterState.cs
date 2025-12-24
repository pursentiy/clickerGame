using System.Collections.Generic;
using Handlers;
using Handlers.UISystem;
using Installers;
using Services;
using Storage.Levels.Params;
using Zenject;
using ScreenHandler = Handlers.ScreenHandler;

namespace GameState
{
    public class OnGameEnterState : InjectableMonoBehaviour
    {
        [Inject] private LevelsParamsStorage _levelsParamsStorage;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private ProfileBuilderService _profileBuilderService;
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private PlayerSnapshotService _playerSnapshotService;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;

        protected override void Awake()
        {
            _applicationService.RegisterDisposableService(_uiManager);
            
            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            _playerProgressService.InitializeProfileSettings();
            
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
            _playerRepositoryService.SavePlayerSnapshot(playerSnapshot);
                
            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private void StartOldProfileSession()
        {
            _playerSnapshotService.Initialize(_playerRepositoryService.LoadPlayerSnapshot());
            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private List<PackParams> GetNewPacks(List<PackParams> savedDataProgress)
        {
            var diff = _levelsParamsStorage.DefaultPacksParamsList.Count - savedDataProgress.Count;
            var newPacks = new List<PackParams>();
            
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
            _soundHandler.SetMusicVolume(_playerProgressService.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_playerProgressService.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }

        private bool IsNewPacksAdded(List<PackParams> savedDataProgress)
        {
            return _levelsParamsStorage.DefaultPacksParamsList.Count > savedDataProgress.Count;
        }
    }
}