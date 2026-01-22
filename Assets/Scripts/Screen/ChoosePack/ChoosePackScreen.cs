using System.Collections.Generic;
using Common.Currency;
using Common.Data.Info;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Settings;
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
            _headerText.text = _localizationService.GetValue("choose_pack_header");
            
            _availablePacksText.text = _localizationService.GetFormattedValue("unlocked_sets", 
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}");
        }
        
        private void OnInfoButtonClicked()
        {
            var context = new UniversalPopupContext(
                _localizationService.GetValue("unlock_sets_info"),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetValue(LocalizationExtensions.OkKey), null)
                }, _localizationService.GetValue(LocalizationExtensions.InfoTitle));

            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }

        private void OnSettingsButtonClicked()
        {
            var context = new SettingsPopupContext(true);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }

        private void OnGoBackButtonClicked()
        {
            _screenHandler.ShowWelcomeScreen();
        }

        private void InitializePackButtons()
        {
            var packsInfos = _progressProvider.GetAllPacks();
            if (packsInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializePackButtons)}]: {nameof(ProgressProvider)} packs params are null or empty.");
                return;
            }
            
            HorizontalLayoutGroup oldHorizontalLayoutGroup = null;
            var index = 0;
            foreach (var packInfo in packsInfos)
            {
                var horizontalLayoutGroup = TryInstantiateHorizontalLayoutGroup(oldHorizontalLayoutGroup, index);
                oldHorizontalLayoutGroup =  horizontalLayoutGroup;
                
                var packId = packInfo.PackId;
                var enterButton = Instantiate(_packItemWidgetPrefab, horizontalLayoutGroup.transform);
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                
                enterButton.Initialize(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                    () => OnAvailablePackClicked(isUnlocked, packInfo), OnUnavailablePackClicked, starsRequired);
                index++;
            }
            
            void OnAvailablePackClicked(bool isUnlocked, PackInfo packInfo)
            {
                if (!isUnlocked)
                {
                    LoggerService.LogWarning(this, $"[{nameof(OnAvailablePackClicked)}] pack {packInfo.PackName} {packInfo.PackId} is not unlocked.");
                    return;
                }
                        
                _progressController.SetCurrentPackId(packInfo.PackId);
                _screenHandler.ShowChooseLevelScreen(packInfo);
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