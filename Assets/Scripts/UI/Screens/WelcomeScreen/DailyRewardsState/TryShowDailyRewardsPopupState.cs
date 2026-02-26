using Controllers;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.DailyReward;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.DailyRewardsState
{
    public class TryShowDailyRewardsPopupState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly DailyRewardsInfoProvider _dailyRewardsInfoProvider;
        [Inject] private readonly DailyRewardController _dailyRewardController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly CoroutineService _coroutineService;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            TryResetDailyRewardsStreak();
            
            TryShowDailyRewardPopup()
                .Then(NextState)
                .CancelWith(this);
        }
        
        private void TryResetDailyRewardsStreak()
        {
            if (_dailyRewardsInfoProvider.IsStreakBrokenAndNotReset() && !_dailyRewardController.TryResetDailyRewardsStreak())
                _dailyRewardController.SaveResetSnapshot();
        }

        private IPromise<DailyRewardsAcquireInfo> TryShowDailyRewardPopup()
        {
            if (!_dailyRewardsInfoProvider.TryGetDailyRewardPopupInfo(out var rewardInfo))
                return Promise<DailyRewardsAcquireInfo>.Resolved(null);

            var claimedPromise = new Promise<bool>();
            var context = new Popups.DailyRewardPopup.DailyRewardPopupContext(
                rewardInfo.DayIndex,
                rewardInfo.RewardsByDay,
                rewardInfo.EarnedDailyReward,
                claimedPromise);

            var info = _flowPopupController.ShowDailyRewardPopup(context, PopupShowingOptions.Enqueue);

            return info.MediatorHidePromise
                .Then(() => context.ClaimedRewards)
                .Then(claimed => Promise<DailyRewardsAcquireInfo>.Resolved(
                    claimed && context.EarnedDailyReward is { Count: > 0 }
                        ? new DailyRewardsAcquireInfo(context.EarnedDailyReward)
                        : null))
                .CancelWith(this);
        }

        private void NextState(DailyRewardsAcquireInfo rewardsAcquireInfo)
        {
            Sequence.ActivateState<TryAcquireDailyRewardsState>(rewardsAcquireInfo);
        }
    }
}