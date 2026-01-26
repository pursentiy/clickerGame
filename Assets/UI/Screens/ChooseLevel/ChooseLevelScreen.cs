using Common.Data.Info;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Services;
using Services.Player;
using Services.ScreenObserver;
using TMPro;
using UI.Popups.MessagePopup;
using UI.Popups.SettingsPopup;
using UI.Popups.UniversalPopup;
using UI.Screens.ChooseLevel.Widgets;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.ChooseLevel
{
    public class ChooseLevelScreen : ScreenBase
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScreenObserverService _screenObserverService;
        
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
        private PackInfo _currentPackInfo;

        public void Initialize(PackInfo packInfo)
        {
            _currentPackId = _progressController.CurrentPackId;
            _currentPackInfo = packInfo;
            
            SetTexts();
            InitializeLevelsButton();
            SetAvailableLevelsText();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            _goBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            
            _screenObserverService.OnOrientationChangeSignal.MapListener(_ => HideAllInfoMessagesPopups()).DisposeWith(this);
            _screenObserverService.OnResolutionChangeSignal.MapListener(HideAllInfoMessagesPopups).DisposeWith(this);
        }

        private void OnSettingsButtonClicked()
        {
            var context = new SettingsPopupContext(true);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }

        private void OnGoBackButtonClicked()
        {
            _screenHandler.ShowChoosePackScreen();
        }

        private void SetTexts()
        {
            _headerText.text = _localizationService.GetValue("choose_level_header");
            
            var localizedName = _localizationService.GetValue($"pack_{_currentPackInfo.PackName.ToLower()}");
            var wordPack = _localizationService.GetValue("word_pack");
            _packName.text = $"{localizedName} {wordPack}";
        }

        private void OnInfoButtonClicked()
        {
            var fontSize = 150;
            var context = new MessagePopupContext(_localizationService.GetValue("unlocked_levels_info"), _infoButton.GetRectTransform(), fontSize);
            _uiManager.PopupsHandler.ShowPopupImmediately<MessagePopupMediator>(context)
                .CancelWith(this);
        }
        
        private void SetAvailableLevelsText()
        {
            var totalLevels = _progressProvider.GetLevelsCountInPack(_currentPackId);
            var totalAvailableLevels = _progressProvider.GetLevelsCountInPack(_currentPackId, true);
            
            _availableLevelsText.text = _localizationService.GetFormattedValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}");
        }

        private void InitializeLevelsButton()
        {
            var index = 0;
            _currentPackInfo.LevelsInfo.ForEach(levelParams =>
            {
                if (_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);

                var enterButton = Instantiate(_levelItemWidgetPrefab, _horizontalGroup.transform);
                var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_currentPackId, levelParams.LevelId) ?? 0;
                enterButton.Initialize(levelParams.LevelName, levelParams.LevelImage, earnedStarsForLevel, levelParams.LevelDifficulty, _progressProvider.IsLevelAvailableToPlay(_currentPackId, levelParams.LevelId),
                    () => StartLevel(_currentPackInfo, levelParams));
                index++;
            });
        }

        private void StartLevel(PackInfo packInfo, LevelInfo levelInfo)
        {
            if (levelInfo == null)
            {
                LoggerService.LogError($"{nameof(PackInfo)} is null");
                return;
            }

            _screenHandler.StartNewLevel(levelInfo.LevelId, packInfo, levelInfo);
        }

        private void OnDestroy()
        {
            HideAllInfoMessagesPopups();
        }
        
        private void HideAllInfoMessagesPopups()
        {
            _uiManager.PopupsHandler.HideAllPopups<MessagePopupMediator>();
        }
    }
}