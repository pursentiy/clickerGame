using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
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
            var context = new MessagePopupContext("За просмотр рекламы вы можете получить 10 монет!", Context.AdsButtonWidget.RectTransform);
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