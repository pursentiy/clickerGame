using Controllers;
using Extensions;
using Handlers.UISystem;
using RSG;
using Services;
using Services.Configuration;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.Player;
using Services.ScreenBlocker;
using UI.Popups.MessagePopup;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.NoCurrencySequence
{
    public class VisualizeNotEnoughCurrencyState : InjectableStateBase<VisualizeNotEnoughCurrencyContext>
    {
        private const float MessagePopupFontSize = 175f;
        
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly GameInfoProvider _gameInfoProvider;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;
        [Inject] private readonly FlowPopupController _flowPopupController;

        private IUIBlockRef _uiBlockRef;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            EnableBlocker();
            
            VisualizeCurrencyWidgetAnimation()
                .Then(() =>
                {
                    VisualizeAdsButton();
                    return ShowMessagePopup();
                })
                .Then(flowInfo =>
                {
                    flowInfo.MediatorLoadPromise.ContinueWithResolved(DisposeBlocker).CancelWith(this);
                    return flowInfo.MediatorHidePromise;
                })
                .ContinueWithResolved(() =>
                {
                    DisposeBlocker();
                    FinishSequence();
                })
                .CancelWith(this);
        }

        private IPromise VisualizeCurrencyWidgetAnimation()
        {
            return Context.CurrencyDisplayWidget.Bump();
        }

        private IPromise<MediatorFlowInfo> ShowMessagePopup()
        {
            var currencyToEarnViaAds = _gameInfoProvider.StarsRewardForAds;
            var spriteAsset = _currencyLibraryService.GetSpriteAsset(CurrencyExtensions.StarsCurrencyName);
            var context = new MessagePopupContext(_localizationService.GetFormattedValue(LocalizationExtensions.AdsInfo, currencyToEarnViaAds), Context.AdsButtonWidget.RectTransform, MessagePopupFontSize, spriteAsset);
            var flowInfo = _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: this);

            return Promise<MediatorFlowInfo>.Resolved(flowInfo);
        }

        private void VisualizeAdsButton()
        {
            Context.AdsButtonWidget.BumpButton();
        }

        private void EnableBlocker()
        {
            _uiBlockRef = _uiScreenBlocker.Block();
        }

        private void DisposeBlocker()
        {
            _uiBlockRef?.Dispose();
        }
        
        private void FinishSequence()
        {
            Sequence.Finish();
        }
    }
}