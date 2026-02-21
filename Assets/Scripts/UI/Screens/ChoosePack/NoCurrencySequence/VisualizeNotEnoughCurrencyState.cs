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
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly CoroutineService _coroutineService;

        private IUIBlockRef _uiBlockRef;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            EnableBlocker();

            VisualizeCurrencyWidgetAnimation()
                .Then(() =>
                {
                    VisualizeAdsButton();
                    return Context.ShowMessagePopup(this);
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
            return Context.CurrencyDisplayWidget.BumpCurrencies(Context.DesiredCurrency);
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
