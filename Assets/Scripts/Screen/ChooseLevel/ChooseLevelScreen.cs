using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Settings;
using Screen.ChooseLevel.Widgets;
using Services;
using Storage;
using Storage.Levels.Params;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen.ChooseLevel
{
    public class ChooseLevelScreen : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PopupHandler _popupHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        
        [SerializeField] private LevelItemWidget _levelItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private Button _goToChoosePackScreenButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _packName;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        
        private HorizontalLayoutGroup _horizontalGroup;

        private void Start()
        {
            InitializeLevelsButton();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            _packName.text = _figuresStorageData.GetPackParamsData(_playerProgressService.CurrentPackNumber).PackName + " Pack";
            
            _goToChoosePackScreenButton.onClick.MapListenerWithSound(()=> _screenHandler.ShowChoosePackScreen());
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null));
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
                var enterButton = Instantiate(_levelItemWidgetPrefab, _horizontalGroup.transform);
                
                var earnedStarsForLevel = _playerProgressService.GetEarnedStarsForLevel(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber) ?? 0;
                enterButton.Initialize(levelParamsData.LevelName, levelParamsData.LevelImage, earnedStarsForLevel, levelParamsData.LevelDifficulty, _playerProgressService.IsLevelAvailable(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber),
                    () => StartLevel(levelParams));
                index++;
            });
        }

        private void StartLevel(LevelParams levelParams)
        {
            if (levelParams == null)
            {
                LoggerService.LogError($"{nameof(LevelParams)} is null");
                return;
            }

            _screenHandler.StartNewLevel(levelParams.LevelNumber, levelParams);
        }
    }
}