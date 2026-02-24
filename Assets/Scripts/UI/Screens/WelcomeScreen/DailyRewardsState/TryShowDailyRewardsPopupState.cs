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
            if (!_dailyRewardsInfoProvider.TryGetDailyRewardPopupInfo(out var rewardInfo))
                return Promise<DailyRewardsAcquireInfo>.Resolved(null);

            var context = new Popups.DailyRewardPopup.DailyRewardPopupContext(
                rewardInfo.DayIndex,
                rewardInfo.RewardsByDay,
                rewardInfo.EarnedDailyReward);

            var info = _flowPopupController.ShowDailyRewardPopup(context, PopupShowingOptions.Enqueue);

            return info.MediatorHidePromise
                .Then(() => Promise<DailyRewardsAcquireInfo>.Resolved(
                    _dailyRewardsInfoProvider.TryGetTodayRewardPreview(out var preview)
                        ? new DailyRewardsAcquireInfo(preview.EarnedDailyReward)
                        : null))
                .CancelWith(this);
        }

        private void NextState(DailyRewardsAcquireInfo rewardsAcquireInfo)
        {
            Sequence.ActivateState<TryAcquireDailyRewardsState>(rewardsAcquireInfo);
        }
    }
}