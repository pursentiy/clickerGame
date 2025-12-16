using Handlers;
using Screen.SubElements;
using Services;
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
        [Inject] private PopupHandler _popupHandler;
        [Inject] private ProgressService _progressService;
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private SoundHandler _soundHandler;
        
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

            _packName.text = _figuresStorageData.GetPackParamsData(_progressService.CurrentPackNumber).PackName + " Pack";
            
            _goToChoosePackScreenButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _screenHandler.ShowChoosePackScreen();
            });
            _settingsButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _popupHandler.ShowSettings();
            });
        }

        private void InitializeLevelsButton()
        {
            var levelsParams = _progressService.GetLevelsByPack(_progressService.CurrentPackNumber);
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);

                var levelParamsData = _figuresStorageData.GetLevelParamsData(_progressService.CurrentPackNumber, levelParams.LevelNumber);
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
            _settingsButton.onClick.RemoveAllListeners();
        }
    }
}