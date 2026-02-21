using System;
using Common.Currency;
using Extensions;
using RSG;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.AdsSequence
{
    public class VisualizeAdsRewardsState : InjectableStateBase<RewardedAdsSequenceContext, RewardsEarnedInfo>
    {
        private const float ScreenBlockTime = 300;
        
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private readonly CoroutineService _coroutineService;

        private IUIBlockRef _uiBlockRef;
        
        private bool HaveAnyAdsRewards => TypedArgument.EarnedCurrency != null && TypedArgument.EarnedCurrency.GetCount() > 0 && TypedArgument.NewTotalCurrency.GetCount() > 0;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            if (!HaveAnyAdsRewards)
            {
                FinishSequence();
                return;
            }

            PrepareEnvironment();
            
            VisualizeRewardsFlight(TypedArgument.EarnedCurrency)
                .Then(() => VisualizeRewardsUpdate(TypedArgument.NewTotalCurrency))
                .ContinueWithResolved(FinishSequence)
                .CancelWith(this);
        }

        private IPromise VisualizeRewardsFlight(ICurrency totalStars)
        {
            var context = new FlyingUIRewardAnimationContext(
                new [] {totalStars}, 
                Context.AdsRewardsVisualizationContainer, 
                new [] {Context.AdsButtonTransform.position},
                new [] {Context.CurrencyDisplayWidget.GetAnimationTarget(totalStars)},
                rewardsMoveTimeSpeedupFactor: 2f,
                spawnSettings: new ValueTuple<float, float>(1f, 3f));

            return _flyingUIRewardAnimationService.PlayAnimation(context)
                .CancelWith(this);
        }

        private IPromise VisualizeRewardsUpdate(ICurrency totalStars)
        {
            Context.CurrencyDisplayWidget.SetCurrency(totalStars, true);

            return _coroutineService.WaitFor(0.2f) 
                .Then(Context.UpdatePacksAction.SafeInvoke)
                .CancelWith(this);
        }

        private void FinishSequence()
        {
            RevertEnvironment();
            Sequence.Finish();
        }

        private void PrepareEnvironment()
        {
            _uiBlockRef = _uiScreenBlocker.Block(ScreenBlockTime);
        }

        private void RevertEnvironment()
        {
            _uiBlockRef?.Dispose();
        }
    }

    public struct RewardsEarnedInfo
    {
        public ICurrency NewTotalCurrency;
        public ICurrency EarnedCurrency;

        public RewardsEarnedInfo(ICurrency newTotalCurrency, ICurrency earnedCurrency)
        {
            NewTotalCurrency = newTotalCurrency;
            EarnedCurrency = earnedCurrency;
        }
    }
}