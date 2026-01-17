using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Universal;
using Screen.ChooseLevel.Widgets;
using Services;
using Services.Player;
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
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
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
        private int _currentPackId;
        private PackParamsData _currentPackParams;

        public void Initialize(PackParamsData packParams)
        {
            _currentPackId = _progressController.CurrentPackId;
            _currentPackParams = packParams;
            
            SetTexts();
            InitializeLevelsButton();
            SetAvailableLevelsText();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            _goBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
        }

        private void OnSettingsButtonClicked()
        {
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null);
        }

        private void OnGoBackButtonClicked()
        {
            _screenHandler.ShowChoosePackScreen();
        }

        private void SetTexts()
        {
            _headerText.text = _localizationService.GetGameValue("choose_level_header");
            
            var localizedName = _localizationService.GetGameValue($"pack_{_currentPackParams.PackName.ToLower()}");
            var wordPack = _localizationService.GetCommonValue("word_pack");
            _packName.text = $"{localizedName} {wordPack}";
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
            var totalLevels = _progressProvider.GetLevelsCountInPack(_currentPackId);
            var totalAvailableLevels = _progressProvider.GetLevelsCountInPack(_currentPackId, true);
            
            _availableLevelsText.text = _localizationService.GetFormattedCommonValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}");
        }

        private void InitializeLevelsButton()
        {
            var index = 0;
            _currentPackParams.LevelsParams.ForEach(levelParams =>
            {
                if (_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);

                var enterButton = Instantiate(_levelItemWidgetPrefab, _horizontalGroup.transform);
                var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_currentPackId, levelParams.LevelId) ?? 0;
                enterButton.Initialize(levelParams.LevelName, levelParams.LevelImage, earnedStarsForLevel, levelParams.LevelDifficulty, _progressProvider.IsLevelAvailableToPlay(_currentPackId, levelParams.LevelId),
                    () => StartLevel(_currentPackParams, levelParams));
                index++;
            });
        }

        private void StartLevel(PackParamsData packParams, LevelParamsData levelParams)
        {
            if (levelParams == null)
            {
                LoggerService.LogError($"{nameof(LevelParamsData)} is null");
                return;
            }

            _screenHandler.StartNewLevel(levelParams.LevelId, packParams, levelParams);
        }

        private void OnDestroy()
        {
            _uiManager.PopupsHandler.HideAllPopups<UniversalPopupMediator>();
        }
    }
}