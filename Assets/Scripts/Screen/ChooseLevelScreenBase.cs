using Handlers;
using Screen.SubElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class ChooseLevelScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private LevelEnterWidgetHandler levelEnterWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private Button _goToChoosePackScreenButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _packName;
        
        private HorizontalLayoutGroup _horizontalGroup;

        private void Start()
        {
            InitializeLevelsButton();

            _packName.text = _progressHandler.GetPackPackByNumber(_progressHandler.CurrentPackNumber).PackName + " Pack";
            
            _goToChoosePackScreenButton.onClick.AddListener(()=> _screenHandler.ShowChoosePackScreen());
        }

        private void InitializeLevelsButton()
        {
            var levelsParams = _progressHandler.GetLevelsByPack(_progressHandler.CurrentPackNumber);
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                
                var enterButton = Instantiate(levelEnterWidgetPrefab, _horizontalGroup.transform);
                enterButton.Initialize(levelParams.LevelName, levelParams.LevelDifficulty, levelParams.LevelPlayable,
                    () =>
                    {
                        _progressHandler.CurrentLevelNumber = levelParams.LevelNumber;
                        _screenHandler.PopupAllScreenHandlers();
                        _levelSessionHandler.StartLevel(levelParams,
                            _levelParamsHandler.LevelHudHandlerPrefab,
                            _levelParamsHandler.TargetFigureDefaultColor);
                    });
                
                index++;
            });
        }
        
        private void OnDestroy()
        {
            _goToChoosePackScreenButton.onClick.RemoveAllListeners();
        }
    }
}