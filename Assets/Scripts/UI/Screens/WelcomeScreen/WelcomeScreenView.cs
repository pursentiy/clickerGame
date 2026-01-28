using Handlers.UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.WelcomeScreen
{
    public class WelcomeScreenView : MonoBehaviour, IUIView
    {
        public Button PlayButton;
        public Button SettingsButton;
        public RectTransform HeaderText;
        public CanvasGroup HeaderTextCanvasGroup;
        
        [Header("Animation Settings")]
        public float Duration = 0.8f;
        public float StartScale = 0.5f;
        public float FlyOffset = 50f;
    }
}