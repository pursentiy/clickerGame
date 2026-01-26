using System;
using Common.Currency;
using Extensions;
using Handlers;
using RSG;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.AdsSequence
{
    public class VisualizeAdsRewardsState : InjectableStateBase<RewardedAdsSequenceContext, RewardsEarnedInfo>
    {
        [Inject] private readonly UIBlockHandler _uiBlockHandler;
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private readonly CoroutineService _coroutineService;

        private bool HaveAnyAdsRewards => TypedArgument.EarnedCurrency != null && TypedArgument.EarnedCurrency.GetCount() > 0 && TypedArgument.NewTotalCurrency.GetCount() > 0;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            if (!HaveAnyAdsRewards)
            {
                FinishSequence();
                return;
            }

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
                new [] {Context.CurrencyDisplayWidget.AnimationTarget.position},
                rewardsMoveTimeSpeedupFactor: 2f,
                spawnSettings: new ValueTuple<float, float>(1f, 3.5f));

            return _flyingUIRewardAnimationService.PlayAnimation(context)
                .CancelWith(this);
        }

        private IPromise VisualizeRewardsUpdate(ICurrency totalStars)
        {
            Context.CurrencyDisplayWidget.SetCurrency(totalStars.GetCount(), true);

            return _coroutineService.WaitFor(0.2f) 
                .Then(Context.UpdatePacksAction.SafeInvoke)
                .CancelWith(this);
        }

        private void FinishSequence()
        {
            RevertEnvironment();
            Sequence.Finish();
        }

        private void RevertEnvironment()
        {
            _uiBlockHandler.BlockUserInput(false);
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