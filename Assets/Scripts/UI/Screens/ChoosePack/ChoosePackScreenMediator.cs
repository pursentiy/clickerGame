using System.Collections.Generic;
using Attributes;
using Common.Currency;
using Common.Data.Info;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Services;
using Services.Player;
using UI.Popups.MessagePopup;
using UI.Popups.SettingsPopup;
using UI.Screens.ChoosePack.AdsSequence;
using UI.Screens.ChoosePack.NoCurrencySequence;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack
{
    [AssetKey("UI Screens/ChoosePackScreenMediator")]
    public sealed class ChoosePackScreenMediator : UIScreenBase<ChoosePackScreenView>
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly LocalizationService _localizationService;
        
        private List<HorizontalLayoutGroup> _horizontalGroups = new();
        private List<PackItemWidget> _packItems = new();
        
        public override void OnCreated()
        {
            base.OnCreated();

            View.StarsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);

            InitText();
            InitializePackButtons();
            
            View.InfoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            View.GoBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.AdsButton.Button.onClick.MapListenerWithSound(OnAdsButtonClicked).DisposeWith(this);
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
            View.HeaderText.text = _localizationService.GetValue("choose_pack_header");
            
            View.AvailablePacksText.text = _localizationService.GetFormattedValue("unlocked_sets", 
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}");
        }
        
        private void OnInfoButtonClicked()
        {
            var fontSize = 150;
            var context = new MessagePopupContext(_localizationService.GetValue("unlock_sets_info"), View.InfoButton.GetRectTransform(), fontSize, facing: PopupFacing.Right);
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
            
            HorizontalLayoutGroup oldHorizontalLayoutGroup = null;
            var index = 0;
            foreach (var packInfo in packsInfos)
            {
                var horizontalLayoutGroup = TryInstantiateHorizontalLayoutGroup(oldHorizontalLayoutGroup, index);
                oldHorizontalLayoutGroup =  horizontalLayoutGroup;
                
                var packId = packInfo.PackId;
                var packItemWidget = Instantiate(View.PackItemWidgetPrefab, horizontalLayoutGroup.transform);
                _packItems.Add(packItemWidget);
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                
                packItemWidget.Initialize(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                    () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
                index++;
            }
            
            HorizontalLayoutGroup TryInstantiateHorizontalLayoutGroup(HorizontalLayoutGroup maybeHorizontalLayoutGroup, int itemIndex)
            {
                if (maybeHorizontalLayoutGroup == null || itemIndex % View.RowPacksCount == 0)
                {
                    var group = Instantiate(View.HorizontalLayoutGroupPrefab, View.LevelEnterPopupsParentTransform);
                    _horizontalGroups.Add(group);
                    return group;
                }

                return maybeHorizontalLayoutGroup;
            }
        }
        
        private void OnAdsButtonClicked()
        {
            StateMachine
                .CreateMachine(new RewardedAdsSequenceContext(View.StarsDisplayWidget, UpdatePacksState, View.AdsButton.RectTransform, this.GetRectTransform()))
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
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(View.StarsDisplayWidget, View.AdsButton))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
        }

        public override void OnEndHide()
        {
            base.OnEndHide();
            
            HideAllInfoMessagesPopups();
        }

        private void HideAllInfoMessagesPopups()
        {
            _uiManager.PopupsHandler.HideAllPopups<MessagePopupMediator>();
        }
    }
}