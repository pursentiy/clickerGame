using System.Collections.Generic;
using Common.Currency;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Universal;
using Screen.ChoosePack.Widgets;
using Services;
using Services.Player;
using Storage.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.ChoosePack
{
    public class ChoosePackScreen : ScreenBase
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly LocalizationService _localizationService;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _availablePacksText;
        [SerializeField] private Button _goBack;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;
        [Range(1, 5)]
        [SerializeField] private int _rowPacksCount = 2;

        private List<HorizontalLayoutGroup> _horizontalGroups = new();

        protected override void Start()
        {
            base.Start();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);

            InitText();
            InitializePackButtons();
            
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            _goBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
        }

        private void InitText()
        {
            _headerText.text = _localizationService.GetGameValue("choose_pack_header");
            
            _availablePacksText.text = _localizationService.GetFormattedCommonValue("unlocked_sets", 
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}");
        }
        
        private void OnInfoButtonClicked()
        {
            var context = new UniversalPopupContext(
                _localizationService.GetCommonValue("unlock_sets_info"),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.OkKey), null)
                }, _localizationService.GetCommonValue(LocalizationExtensions.InfoTitle));

            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }

        private void OnSettingsButtonClicked()
        {
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null);
        }

        private void OnGoBackButtonClicked()
        {
            _screenHandler.ShowWelcomeScreen();
        }

        private void InitializePackButtons()
        {
            var currentPackParams = _progressProvider.GetAllPacks();
            if (currentPackParams.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializePackButtons)}]: {nameof(ProgressProvider)} packs params are null or empty.");
                return;
            }
            
            HorizontalLayoutGroup oldHorizontalLayoutGroup = null;
            var index = 0;
            foreach (var packParams in currentPackParams)
            {
                var horizontalLayoutGroup = TryInstantiateHorizontalLayoutGroup(oldHorizontalLayoutGroup, index);
                oldHorizontalLayoutGroup =  horizontalLayoutGroup;
                
                var packId = packParams.PackId;
                var enterButton = Instantiate(_packItemWidgetPrefab, horizontalLayoutGroup.transform);
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                
                enterButton.Initialize(packParams.PackName, packParams.PackImagePrefab, packId, isUnlocked,
                    () => OnAvailablePackClicked(isUnlocked, packParams), OnUnavailablePackClicked, starsRequired);
                index++;
            }
            
            void OnAvailablePackClicked(bool isUnlocked, PackParamsData packParams)
            {
                if (!isUnlocked)
                {
                    LoggerService.LogWarning(this, $"[{nameof(OnAvailablePackClicked)}] pack {packParams.PackName} {packParams.PackId} is not unlocked.");
                    return;
                }
                        
                _progressController.SetCurrentPackId(packParams.PackId);
                _screenHandler.ShowChooseLevelScreen(packParams);
            }
            
            void OnUnavailablePackClicked()
            {
                _starsDisplayWidget.Bump();
            }

            HorizontalLayoutGroup TryInstantiateHorizontalLayoutGroup(HorizontalLayoutGroup maybeHorizontalLayoutGroup, int itemIndex)
            {
                if (maybeHorizontalLayoutGroup == null || itemIndex % _rowPacksCount == 0)
                {
                    var group = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                    _horizontalGroups.Add(group);
                    return group;
                }

                return maybeHorizontalLayoutGroup;
            }
        }
        
        private void OnDestroy()
        {
            _uiManager.PopupsHandler.HideAllPopups<UniversalPopupMediator>();
        }
    }
}