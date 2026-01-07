using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Settings;
using Screen.ChooseLevel.Widgets;
using Services;
using Storage;
using Storage.Levels;
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
        [Inject] private LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        [Inject] private LocalizationService _localization;
        
        [SerializeField] private LevelItemWidget _levelItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private Button _goToChoosePackScreenButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _packName;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        
        private HorizontalLayoutGroup _horizontalGroup;

        private void Start()
        {
            _headerText.text = _localization.GetGameValue("choose_level_header");

            InitializeLevelsButton();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            var rawPackName = _levelsParamsStorageData.GetPackParamsData(_playerProgressService.CurrentPackNumber).PackName;
            var localizedName = _localization.GetGameValue($"pack_{rawPackName.ToLower()}");
            var wordPack = _localization.GetCommonValue("word_pack");
            _packName.text = $"{localizedName} {wordPack}";
            
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

                var levelParamsData = _levelsParamsStorageData.GetLevelParamsData(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber);
                var enterButton = Instantiate(_levelItemWidgetPrefab, _horizontalGroup.transform);
                
                var earnedStarsForLevel = _playerProgressService.GetEarnedStarsForLevel(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber) ?? 0;
                enterButton.Initialize(levelParamsData.LevelName, levelParamsData.LevelImage, earnedStarsForLevel, levelParamsData.LevelDifficulty, _playerProgressService.IsLevelAvailable(_playerProgressService.CurrentPackNumber, levelParams.LevelNumber),
                    () => StartLevel(levelParams));
                index++;
            });
        }

        private void StartLevel(LevelParamsData levelParams)
        {
            if (levelParams == null)
            {
                LoggerService.LogError($"{nameof(LevelParamsData)} is null");
                return;
            }

            _screenHandler.StartNewLevel(levelParams.LevelNumber, levelParams);
        }
    }
}