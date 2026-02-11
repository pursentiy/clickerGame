using Attributes;
using Controllers;
using Extensions;
using Handlers.UISystem.Screens;
using Services;
using UI.Popups.MessagePopup;
using UI.Screens.WelcomeScreen.AuthenticateSequence;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen
{
    [AssetKey("UI Screens/WelcomeScreenScreenMediator")]
    public sealed class WelcomeScreenMediator : UIScreenBase<WelcomeScreenView>
    {
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly DailyRewardService _dailyRewardService;

        public override void OnCreated()
        {
            base.OnCreated();

            View.PlayButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.LoginButtonWidget.Initialize();
            
            if (View.DailyRewardsButton != null)
            {
                View.DailyRewardsButton.Initialize();
            }
        }

        public override void OnBeginShow()
        {
            base.OnBeginShow();

            View.ScreenAnimationWidget.ShowAnimation();
            
            TryShowDailyRewardPopup();
            
            if (View.DailyRewardsButton != null)
            {
                View.DailyRewardsButton.UpdateTimer();
            }
        }

        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void PushNextScreen()
        {
            _flowScreenController.GoToChoosePackScreen();
        }

        private void TryShowDailyRewardPopup()
        {
            if (_dailyRewardService.TryGetTodayRewardPreview(out var rewardInfo))
            {
                var context = new UI.Popups.DailyRewardPopup.DailyRewardPopupContext(
                    rewardInfo.DayIndex,
                    rewardInfo.RewardsByDay,
                    rewardInfo.EarnedDailyReward);
                _flowPopupController.ShowDailyRewardPopup(context, PopupShowingOptions.Enqueue);
            }
        }
    }
}