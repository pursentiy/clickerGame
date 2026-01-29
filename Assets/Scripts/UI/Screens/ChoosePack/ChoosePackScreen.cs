using System.Collections;
using System.Collections.Generic;
using Common.Currency;
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
using UI.Screens.ChoosePack.AdsSequence;
using UI.Screens.ChoosePack.NoCurrencySequence;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack
{
    public class ChoosePackScreen : ScreenBase
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly ScreenObserverService _screenObserverService;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _availablePacksText;
        [SerializeField] private Button _goBack;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private AdsButtonWidget _adsButton;
        [Range(1, 5)]
        [SerializeField] private int _rowPacksCount = 2;

        private List<HorizontalLayoutGroup> _horizontalGroups = new();
        private List<PackItemWidget> _packItems = new();

        protected override void Start()
        {
            base.Start();

            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);

            InitText();
            InitializePackButtons();
            
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            _goBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            _adsButton.Button.onClick.MapListenerWithSound(OnAdsButtonClicked).DisposeWith(this);
            
            _screenObserverService.OnOrientationChangeSignal.MapListener(_ => HideAllInfoMessagesPopups()).DisposeWith(this);
            _screenObserverService.OnResolutionChangeSignal.MapListener(HideAllInfoMessagesPopups).DisposeWith(this);
        }

        private void UpdatePacksState()
        {
            if (_packItems.IsCollectionNullOrEmpty())
                return;

            foreach (var packItemWidget in _packItems)
            {
                if (packItemWidget == null)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: {nameof(PackItemWidget)} is null");
                    continue;
                }
                
                var packId = packItemWidget.PackId;
                var packInfo = _progressProvider.GetPackInfo(packId);
                if (packInfo == null)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: {nameof(PackInfo)} is null for pack id {packId}");
                    continue;
                }
                
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                if (!maybeStarsRequired.HasValue)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: cannot get stars for unlocking pack with id {packId}");
                }
                
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                packItemWidget.UpdateState(isUnlocked, () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
            }
        }

        private void InitText()
        {
            _headerText.text = _localizationService.GetValue("choose_pack_header");
            _availablePacksText.SetText(_localizationService.GetFormattedValue("unlocked_sets", 
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}"));
        }
        
        private void OnInfoButtonClicked()
        {
            var fontSize = 150;
            var context = new MessagePopupContext(_localizationService.GetValue("unlock_sets_info"), _infoButton.GetRectTransform(), fontSize, facing: PopupFacing.Right);
            _uiManager.PopupsHandler.ShowPopupImmediately<MessagePopupMediator>(context)
                .CancelWith(this);
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
            
            StartCoroutine(InitializePacksRoutine(packsInfos));
        }
        
        private IEnumerator InitializePacksRoutine(IEnumerable<PackInfo> packsInfos)
        {
            HorizontalLayoutGroup oldHorizontalLayoutGroup = null;
            var index = 0;

            foreach (var packInfo in packsInfos)
            {
                if (this == null || gameObject == null)
                    yield break;

                var horizontalLayoutGroup = TryInstantiateHorizontalLayoutGroup(oldHorizontalLayoutGroup, index);
                oldHorizontalLayoutGroup = horizontalLayoutGroup;

                var packId = packInfo.PackId;
                var packItemWidget = Instantiate(_packItemWidgetPrefab, horizontalLayoutGroup.transform);
                _packItems.Add(packItemWidget);
        
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                
                packItemWidget.Initialize(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                    () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
        
                index++;

                yield return new WaitForSecondsRealtime(0.05f);
            }
        }
        
        private HorizontalLayoutGroup TryInstantiateHorizontalLayoutGroup(HorizontalLayoutGroup maybeHorizontalLayoutGroup, int itemIndex)
        {
            if (maybeHorizontalLayoutGroup == null || itemIndex % _rowPacksCount == 0)
            {
                var group = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                _horizontalGroups.Add(group);
                return group;
            }

            return maybeHorizontalLayoutGroup;
        }
        
        private void OnAdsButtonClicked()
        {
            StateMachine
                .CreateMachine(new RewardedAdsSequenceContext(_starsDisplayWidget, UpdatePacksState, _adsButton.RectTransform, this.GetRectTransform()))
                .StartSequence<TryShowAdsRewardState>()
                .FinishWith(this);
        }
        
        private void OnAvailablePackClicked(PackInfo packInfo)
        {
            _progressController.SetCurrentPackId(packInfo.PackId);
            _screenHandler.ShowChooseLevelScreen(packInfo);
        }
            
        private void OnUnavailablePackClicked()
        {
            StateMachine
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(_starsDisplayWidget, _adsButton))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
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