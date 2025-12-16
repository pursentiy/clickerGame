using System.Collections.Generic;
using Handlers;
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
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private ProfileBuilderService _profileBuilderService;
        [Inject] private PlayerRepositoryService _playerRepositoryService;

        protected override void Awake()
        {
            _progressHandler.InitializeProfileSettings();
            
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
                
            _progressHandler.InitializeHandler(StartNewGameProgress());
        }

        private void StartOldProfileSession()
        {
            _progressHandler.InitializeHandler(savedDataProgress,
                IsNewPacksAdded(savedDataProgress) ? GetNewPacks(savedDataProgress) : null);
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
            _soundHandler.SetMusicVolume(_progressHandler.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_progressHandler.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }

        private bool IsNewPacksAdded(List<PackParams> savedDataProgress)
        {
            return _levelsParamsStorage.DefaultPacksParamsList.Count > savedDataProgress.Count;
        }

        private List<PackParams> StartNewGameProgress()
        {
            var levelsParams = _levelsParamsStorage.DefaultPacksParamsList;
            return levelsParams;
        }

    }
}