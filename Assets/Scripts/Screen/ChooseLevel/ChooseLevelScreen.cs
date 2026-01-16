using System;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Universal;
using Screen.ChooseLevel.Widgets;
using Services;
using Services.Player;
using Storage;
using Storage.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.ChooseLevel
{
    public class ChooseLevelScreen : ScreenBase
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
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
            
            var rawPackName = _levelsParamsStorageData.GetPackParamsData(_progressProvider.CurrentPackNumber).PackName;
            var localizedName = _localization.GetGameValue($"pack_{rawPackName.ToLower()}");
            var wordPack = _localization.GetCommonValue("word_pack");
            _packName.text = $"{localizedName} {wordPack}";
            
            _goBack.onClick.MapListenerWithSound(()=> _screenHandler.ShowChoosePackScreen()).DisposeWith(this);
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null)).DisposeWith(this);
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
            var totalLevels = _progressProvider.GetLevelsCountInPack(_progressProvider.CurrentPackNumber);
            var totalAvailableLevels = _progressProvider.GetAllAvailableLevelsCount(_progressProvider.CurrentPackNumber);
            _availableLevelsText.text = _localization.GetFormattedCommonValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}");
        }

        private void InitializeLevelsButton()
        {
            var levelsParams = _progressProvider.GetLevelsByPack(_progressProvider.CurrentPackNumber);
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);

                var levelParamsData = _levelsParamsStorageData.GetLevelParamsData(_progressProvider.CurrentPackNumber, levelParams.LevelNumber);
                var enterButton = Instantiate(_levelItemWidgetPrefab, _horizontalGroup.transform);
                
                var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_progressProvider.CurrentPackNumber, levelParams.LevelNumber) ?? 0;
                enterButton.Initialize(levelParamsData.LevelName, levelParamsData.LevelImage, earnedStarsForLevel, levelParamsData.LevelDifficulty, _progressProvider.IsLevelAvailableToPlay(_progressProvider.CurrentPackNumber, levelParams.LevelNumber),
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

            _screenHandler.StartNewLevel(levelParams.LevelId, levelParams);
        }

        private void OnDestroy()
        {
            _uiManager.PopupsHandler.HideAllPopups<UniversalPopupMediator>();
        }
    }
}