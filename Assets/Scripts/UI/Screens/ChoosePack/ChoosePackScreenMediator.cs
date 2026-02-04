using Attributes;
using Controllers;
using Extensions;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Services;
using Services.FlyingRewardsAnimation;
using Services.Player;
using UI.Popups.MessagePopup;
using UI.Screens.ChoosePack.AdsSequence;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack
{
    [AssetKey("UI Screens/ChoosePackScreenMediator")]
    public sealed class ChoosePackScreenMediator : UIScreenBase<ChoosePackScreenView>
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;
        
        public override void OnCreated()
        {
            base.OnCreated();
            
            InitWidgets();
            InitText();
            
            View.InfoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            View.GoBackButton.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.AdsButton.Button.onClick.MapListenerWithSound(OnAdsButtonClicked).DisposeWith(this);
            View.AdsInfoButton.onClick.MapListenerWithSound(ShowAdsInfoPopup).DisposeWith(this);
        }

        private void InitWidgets()
        {
            View.StarsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            View.PacksWidget.Initialize(View.StarsDisplayWidget, View.AdsButton);
        }

        private void InitText()
        {
            View.HeaderText.SetText(_localizationService.GetValue("choose_pack_header"));

            var text = _localizationService.GetFormattedValue("unlocked_sets",
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}");
            View.AvailablePacksText.SetText(text);
        }
        
        private void OnInfoButtonClicked()
        {
            var context = new MessagePopupContext(_localizationService.GetValue("unlock_sets_info"), View.InfoButton.GetRectTransform(), View.InfoMessageFontSize, facing: PopupFacing.Right);
            _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: this.GetDisposeProvider());
        }
        
        private void ShowAdsInfoPopup()
        {
            var spriteAsset = _currencyLibraryService.GetSpriteAsset(CurrencyExtensions.StarsCurrencyName);
            var context = new MessagePopupContext(_localizationService.GetValue(LocalizationExtensions.AdsFullInfo), View.AdsInfoButton.GetRectTransform(), View.InfoMessageFontSize, spriteAsset, facing: PopupFacing.Left);
            _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: this.GetDisposeProvider());
        }

        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void OnGoBackButtonClicked()
        {
            _flowScreenController.GoToWelcomeScreen();
        }
        
        private void OnAdsButtonClicked()
        {
            StateMachine
                .CreateMachine(new RewardedAdsSequenceContext(View.StarsDisplayWidget, View.PacksWidget.UpdatePacksState, View.AdsButton.RectTransform, this.GetRectTransform()))
                .StartSequence<TryShowAdsRewardState>()
                .FinishWith(this);
        }
    }
}