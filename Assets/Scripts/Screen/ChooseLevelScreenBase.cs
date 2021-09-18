using Handlers;
using Screen.SubElements;
using UnityEngine;
using Zenject;

namespace Screen
{
    public class ChooseLevelScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private LevelEnterPopupHandler _levelEnterPopupPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;

        private void Start()
        {
            InitializeLevelsButton();
        }

        private void InitializeLevelsButton()
        {
            var levelsParams = _progressHandler.GetAllLevelsParams();
            levelsParams.ForEach(levelParams =>
            {
                var enterButton = Instantiate(_levelEnterPopupPrefab, _levelEnterPopupsParentTransform);
                enterButton.Initialize(levelParams.LevelNumber, levelParams.LevelPlayable,
                    () =>
                    {
                        _screenHandler.PopupAllScreenHandlers();
                        _levelSessionHandler.StartLevel(levelParams,
                            _levelParamsHandler.LevelHudHandlerPrefab,
                            _levelParamsHandler.TargetFigureDefaultColor);
                    });
            });
        }
    }
}