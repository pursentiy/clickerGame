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
        
        public override void OnCreated()
        {
            base.OnCreated();

            View.PlayButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            View.LoginButtonWidget.Initialize();
        }

        public override void OnBeginShow()
        {
            base.OnBeginShow();

            View.ScreenAnimationWidget.ShowAnimation();
        }

        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void PushNextScreen()
        {
            _flowScreenController.GoToChoosePackScreen();
        }
    }
}