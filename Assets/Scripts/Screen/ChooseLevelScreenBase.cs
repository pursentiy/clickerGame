using Handlers;
using Screen.SubElements;
using Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class ChooseLevelScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private FiguresStorageData _figuresStorageData;
        
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

            _packName.text = _figuresStorageData.GetPackParamsData(_progressHandler.CurrentPackNumber).PackName + " Pack";
            
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

                var levelParamsData = _figuresStorageData.GetLevelParamsData(_progressHandler.CurrentPackNumber, levelParams.LevelNumber);
                var enterButton = Instantiate(levelEnterWidgetPrefab, _horizontalGroup.transform);
                enterButton.Initialize(levelParamsData.LevelName, levelParamsData.LevelImage, levelParamsData.LevelDifficulty, levelParams.LevelPlayable,
                    () =>
                    {
                        _screenHandler.StartNewLevel(levelParams.LevelNumber, levelParams);
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