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
        [Inject] private IProcessProgressDataService _processProgressDataService;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private SoundHandler _soundHandler;

        protected override void Awake()
        {
            var savedDataProgress = _processProgressDataService.LoadProgress();

            LaunchSession(savedDataProgress);
        }

        private void LaunchSession(List<PackParams> savedDataProgress)
        {
            if (savedDataProgress == null)
            {
                _progressHandler.InitializeHandler(StartNewGameProgress());
            }
            else
            {
                _progressHandler.InitializeHandler(savedDataProgress,
                    IsNewPacksAdded(savedDataProgress) ? GetNewPacks(savedDataProgress) : null);
            }
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
            _soundHandler.StartAmbience();
        }

        private bool IsNewPacksAdded(List<PackParams> savedDataProgress)
        {
            return _levelsParamsStorage.DefaultPacksParamsList.Count > savedDataProgress.Count;
        }

        private List<PackParams> StartNewGameProgress()
        {
            var levelsParams = _levelsParamsStorage.DefaultPacksParamsList;
            _processProgressDataService.SaveProgress(levelsParams);
            return levelsParams;
        }

        private void OnDestroy()
        {
            _processProgressDataService.SaveProgress(_progressHandler.GetCurrentProgress());
        }
    }
}