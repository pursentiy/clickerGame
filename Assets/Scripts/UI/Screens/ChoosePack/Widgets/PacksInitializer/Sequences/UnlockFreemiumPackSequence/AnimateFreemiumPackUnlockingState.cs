using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
using RSG;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class AnimateFreemiumPackUnlockingState : InjectableStateBase<UnlockFreemiumPackSequenceContext, List<ICurrency>>
    {
        private const float ScreenBlockTime = 10;
        
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly CoroutineService _coroutineService;

        private IUIBlockRef _uiBlockRef;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);
            
            PrepareEnvironment();
            
            VisualizeRewardsFlight(Context.PackCost)
                .Then(() => VisualizeRewardsUpdate(TypedArgument))
                .ContinueWithResolved(FinishSequence)
                .CancelWith(this);
        }
        
        private IPromise VisualizeRewardsFlight(List<ICurrency> rewards)
        {
            var rewardPlaces = rewards.Select(Context.CurrencyDisplayWidget.GetAnimationTarget).ToArray();
            
            var context = new FlyingUIRewardAnimationContext(
                rewards.ToArray(), 
                Context.VisualizerFlightRewardsContainer, 
                rewardPlaces,
                new [] {Context.PackTransform.position},
                rewardsMoveTimeSpeedupFactor: 2f,
                spawnSettings: new ValueTuple<float, float>(1f, 3f));

            return _flyingUIRewardAnimationService.PlayAnimation(context)
                .CancelWith(this);
        }
        
        private IPromise VisualizeRewardsUpdate(List<ICurrency> newRewardsValues)
        {
            if (newRewardsValues.IsCollectionNullOrEmpty())
                return Promise.Resolved();
            
            Context.CurrencyDisplayWidget.SetCurrency(newRewardsValues, true);

            return _coroutineService.WaitFor(0.25f) 
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
}