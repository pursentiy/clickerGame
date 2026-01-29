using Attributes;
using Controllers;
using Extensions;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Services;
using Services.Player;
using UI.Popups.MessagePopup;
using UI.Popups.SettingsPopup;
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
        
        public override void OnCreated()
        {
            base.OnCreated();

            View.StarsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);

            InitText();
            
            View.PacksWidget.Initialize(View.StarsDisplayWidget, View.AdsButton);
            View.PacksWidget.InitializePackButtons(View.PacksContainer);
            
            View.InfoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            View.GoBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.AdsButton.Button.onClick.MapListenerWithSound(OnAdsButtonClicked).DisposeWith(this);
        }
        
        public override void OnEndHide()
        {
            base.OnEndHide();
            
            HideAllInfoMessagesPopups();
        }

        private void InitText()
        {
            View.HeaderText.SetText(_localizationService.GetValue("choose_pack_header"));
            
            View.AvailablePacksText.SetText(_localizationService.GetFormattedValue("unlocked_sets", 
                $"{_progressProvider.GetAllAvailablePacksCount()}/{_progressProvider.GetAllPacksCount()}"));
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

        private void HideAllInfoMessagesPopups()
        {
            _uiManager.PopupsHandler.HideAllPopups<MessagePopupMediator>();
        }
    }
}