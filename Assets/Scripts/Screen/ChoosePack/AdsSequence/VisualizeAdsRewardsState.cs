using System;
using Common.Currency;
using Extensions;
using Handlers;
using RSG;
using Services.FlyingRewardsAnimation;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Screen.ChoosePack.AdsSequence
{
    public class VisualizeAdsRewardsState : InjectableStateBase<RewardedAdsSequenceContext, RewardsEarnedInfo>
    {
        [Inject] private readonly UIBlockHandler _uiBlockHandler;
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;

        private bool HaveAnyAdsRewards => TypedArgument.EarnedCurrency != null && TypedArgument.EarnedCurrency.GetCount() > 0 && TypedArgument.NewTotalCurrency.GetCount() > 0;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            if (!HaveAnyAdsRewards)
            {
                FinishSequence();
                return;
            }

            PrepareForState();
            
            VisualizeRewardsFlight(TypedArgument.EarnedCurrency)
                .Then(() => VisualizeRewardsUpdate(TypedArgument.NewTotalCurrency))
                .ContinueWithResolved(() =>
                {
                    ResetState();
                    FinishSequence();
                })
                .CancelWith(this);
        }

        private IPromise VisualizeRewardsFlight(ICurrency totalStars)
        {
            var context = new FlyingUIRewardAnimationContext(
                new [] {totalStars}, 
                Context.AdsRewardsVisualizationContainer, 
                new [] {Context.AdsButtonTransform.position},
                new [] {Context.CurrencyDisplayWidget.AnimationTarget.position},
                rewardsMoveTimeSpeedupFactor: 2.5f,
                spawnSettings: new ValueTuple<float, float>(1f, 2f));

            return _flyingUIRewardAnimationService.PlayAnimation(context)
                .CancelWith(this);
        }

        private IPromise VisualizeRewardsUpdate(ICurrency totalStars)
        {
            Context.CurrencyDisplayWidget.SetCurrency(totalStars.GetCount(), true);
            Context.UpdatePacksAction.SafeInvoke();
            return Promise.Resolved();
        }

        private void FinishSequence()
        {
            Sequence.Finish();
        }

        private void PrepareForState()
        {
            _uiBlockHandler.BlockUserInput(true);
        }

        private void ResetState()
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