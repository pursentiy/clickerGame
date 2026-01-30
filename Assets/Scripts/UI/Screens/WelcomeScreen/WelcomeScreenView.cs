using Handlers.UISystem;
using UI.Screens.WelcomeScreen.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.WelcomeScreen
{
    public class WelcomeScreenView : MonoBehaviour, IUIView
    {
        public Button PlayButton;
        public Button SettingsButton;
        public WelcomeScreenAnimationWidget ScreenAnimationWidget;
    }
}