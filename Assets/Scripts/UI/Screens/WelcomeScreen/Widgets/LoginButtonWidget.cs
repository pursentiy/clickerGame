using Controllers;
using Extensions;
using Installers;
using Services;
using UI.Popups.MessagePopup;
using UI.Screens.WelcomeScreen.AuthenticateSequence;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.Widgets
{
    public class LoginButtonWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly BridgeService _bridgeService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        
        [SerializeField] private Button LoginButton;
        [SerializeField] private Button LoginInfoButton;
        [SerializeField] private float LoginInfoMessageFontSize = 150f;

        public void Initialize()
        {
            SetupLoginButton();
        }
        
        private void SetupLoginButton()
        {
            if (_bridgeService.ShouldAuthenticatePlayer)
            {
                LoginButton.TrySetActive(true);
                LoginButton.onClick.MapListenerWithSound(StartAuthenticationSequence).DisposeWith(this);
                
                LoginInfoButton.onClick.MapListenerWithSound(ShowMessageInfoPopup).DisposeWith(this);
            }
            else
            {
                LoginButton.TrySetActive(false);
            }
        }
        
        private void ShowMessageInfoPopup()
        {
            var context = new MessagePopupContext(_localizationService.GetValue(LocalizationExtensions.LogInInfo), LoginInfoButton.GetRectTransform(), LoginInfoMessageFontSize);
            _flowPopupController.ShowMessagePopup(context);
        }

        private void StartAuthenticationSequence()
        {
            StateMachine.CreateMachine(null)
                .StartSequence<AuthenticatePlayerState>()
                .FinishWith(this);
        }
    }
}