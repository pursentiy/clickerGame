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

        protected override void Awake()
        {
            var savedDataProgress = _processProgressDataService.LoadProgress();
            
            _progressHandler.InitializeHandler(savedDataProgress ?? StartNewGameProgress());
        }

        private void Start()
        {
            _screenHandler.ShowWelcomeScreen(true);
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