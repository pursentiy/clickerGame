using Handlers;
using Handlers.UISystem;
using Popup.Settings;
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
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        
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

            _packName.text = _figuresStorageData.GetPackParamsData(_playerProgressService.CurrentPackNumber).PackName + " Pack";
            
            _goToChoosePackScreenButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _screenHandler.ShowChoosePackScreen();
            });
            _settingsButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null);
            });
        }

        private void InitializeLevelsButton()
        {
            var levelsParams = _playerProgressService.GetLevelsByPack(_playerProgressService.CurrentPackNumber);
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);

                var levelParamsData = _figuresStorageData.GetLevelParamsData(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber);
                var enterButton = Instantiate(levelEnterWidgetPrefab, _horizontalGroup.transform);
                enterButton.Initialize(levelParamsData.LevelName, levelParamsData.LevelImage, levelParamsData.LevelDifficulty, _playerProgressService.IsLevelAvailable(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber),
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