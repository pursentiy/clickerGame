using System;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Universal;
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
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly PopupHandler _popupHandler;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly LocalizationService _localization;
        [Inject] private readonly LocalizationService _localizationService;
        
        [SerializeField] private LevelItemWidget _levelItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private Button _goBack;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _availableLevelsText;
        [SerializeField] private TMP_Text _packName;
        
        private HorizontalLayoutGroup _horizontalGroup;

        protected override void Start()
        {
            base.Start();
            
            _headerText.text = _localization.GetGameValue("choose_level_header");

            InitializeLevelsButton();
            SetAvailableLevelsText();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            var rawPackName = _levelsParamsStorageData.GetPackParamsData(_playerProgressService.CurrentPackNumber).PackName;
            var localizedName = _localization.GetGameValue($"pack_{rawPackName.ToLower()}");
            var wordPack = _localization.GetCommonValue("word_pack");
            _packName.text = $"{localizedName} {wordPack}";
            
            _goBack.onClick.MapListenerWithSound(()=> _screenHandler.ShowChoosePackScreen());
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked);
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null));
        }

        private void OnInfoButtonClicked()
        {
            var context = new UniversalPopupContext(
                _localizationService.GetCommonValue("unlocked_levels_info"),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.OkKey), null)
                }, _localizationService.GetCommonValue(LocalizationExtensions.InfoTitle));

            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }
        
        private void SetAvailableLevelsText()
        {
            var totalLevels = _playerProgressService.GetAllLevelsCount(_playerProgressService.CurrentPackNumber);
            var totalAvailableLevels = _playerProgressService.GetAllAvailableLevelsCount(_playerProgressService.CurrentPackNumber);
            _availableLevelsText.text = _localization.GetFormattedCommonValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}");
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

        private void OnDestroy()
        {
            _uiManager.PopupsHandler.HideAllPopups<UniversalPopupMediator>();
        }
    }
}