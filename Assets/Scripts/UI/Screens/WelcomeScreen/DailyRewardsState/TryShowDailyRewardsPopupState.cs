using Controllers;
using RSG;
using Services;
using Services.CoroutineServices;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.DailyRewardsState
{
    public class TryShowDailyRewardsPopupState : InjectableStateBase<DefaultStateContext>
    {
        [Inject] private readonly DailyRewardService _dailyRewardService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly CoroutineService _coroutineService;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            TryShowDailyRewardPopup()
                .Then(NextState);
        }
        
        private IPromise<DailyRewardsAcquireInfo> TryShowDailyRewardPopup()
        {
            if (_dailyRewardService.TryGetTodayRewardPreview(out var rewardInfo))
            {
                var context = new Popups.DailyRewardPopup.DailyRewardPopupContext(
                    rewardInfo.DayIndex,
                    rewardInfo.RewardsByDay,
                    rewardInfo.EarnedDailyReward);
                
                var info = _flowPopupController.ShowDailyRewardPopup(context, PopupShowingOptions.Enqueue);

                return info.MediatorHidePromise
                    .Then(() => Promise<DailyRewardsAcquireInfo>.Resolved(new DailyRewardsAcquireInfo(rewardInfo.EarnedDailyReward)))
                    .CancelWith(this);
            }

            return Promise<DailyRewardsAcquireInfo>.Resolved(null);
        }

        private void NextState(DailyRewardsAcquireInfo rewardsAcquireInfo)
        {
            Sequence.ActivateState<TryAcquireDailyRewardsState>(rewardsAcquireInfo);
        }
    }
}