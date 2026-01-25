using Extensions;
using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.Player;
using UI.Popups.MessagePopup;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.NoCurrencySequence
{
    public class VisualizeNotEnoughCurrencyState : InjectableStateBase<VisualizeNotEnoughCurrencyContext>
    {
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIBlockHandler _uiBlockHandler;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly GameConfigurationProvider _gameConfigurationProvider;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;


        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareEnvironment();
            
            VisualizeCurrencyWidgetAnimation()
                .Then(() =>
                {
                    VisualizeAdsButton();
                    return ShowMessagePopup();
                })
                .ContinueWithResolved(() =>
                {
                    ResetEnvironment();
                    FinishSequence();
                })
                .CancelWith(this);
        }

        private IPromise VisualizeCurrencyWidgetAnimation()
        {
            return Context.CurrencyDisplayWidget.Bump();
        }

        private IPromise ShowMessagePopup()
        {
            var currencyToEarnViaAds = _gameConfigurationProvider.StarsRewardForAds;
            var spriteAsset = _currencyLibraryService.GetSpriteAsset(CurrencyExtensions.StarsCurrencyName);
            var fontSize = 175;
            var context = new MessagePopupContext(_localizationService.GetFormattedValue(LocalizationExtensions.AdsInfo, currencyToEarnViaAds), Context.AdsButtonWidget.RectTransform, fontSize, spriteAsset);
            _uiManager.PopupsHandler.ShowPopupImmediately<MessagePopupMediator>(context)
                .CancelWith(this);

            return _coroutineService.WaitFor(0.25f).CancelWith(this);
        }

        private void VisualizeAdsButton()
        {
            Context.AdsButtonWidget.BumpButton();
        }

        private void PrepareEnvironment()
        {
            _uiBlockHandler.BlockUserInput(true);
        }

        private void ResetEnvironment()
        {
            _uiBlockHandler.BlockUserInput(false);
        }
        
        private void FinishSequence()
        {
            Sequence.Finish();
        }
    }
}