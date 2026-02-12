using System.Collections.Generic;
using Common.Currency;
using RSG;
using Services.CoroutineServices;
using Services.Player;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.DailyRewardsState
{
    public class AcquireDailyRewardsState : InjectableStateBase<DefaultStateContext, DailyRewardsAcquireInfo>
    {
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        [Inject] CoroutineService _coroutineService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        
        private IUIBlockRef _uiBlockRef;

        private bool CanAcquireDailyRewards => TypedArgument is { EarnedDailyReward: { Count: > 0 } };
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareEnvironment();
            if (CanAcquireDailyRewards)
            {
                AcquireEarnedStars(TypedArgument.EarnedDailyReward)
                    .ContinueWithResolved(FinishSequence)
                    .CancelWith(this);
            }
            else
            {
                FinishSequence();
            }
        }
        
        private IPromise AcquireEarnedStars(IList<ICurrency> earnedDailyReward)
        {
            foreach (var currency in earnedDailyReward)
            {
                _playerCurrencyService.TryAddCurrency(currency, CurrencyChangeMode.Animated);
            }
            
            return _coroutineService.WaitFrame();
        }
        
        private void PrepareEnvironment()
        {
            _uiBlockRef = _uiScreenBlocker.Block();
        }

        private void RevertEnvironment()
        {
            _uiBlockRef?.Dispose();
        }

        private void FinishSequence()
        {
            RevertEnvironment();
            Sequence.Finish();
        }
    }
}