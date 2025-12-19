using Handlers.UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace Popup.Settings
{
    public class SettingsPopupView : MonoBehaviour, IUIView
    {
        public Button CloseButton;
        public Toggle MusicToggle;
        public Toggle SoundToggle;
        public RectTransform MainTransform;
        public Button LeftLanguageButton;
        public Button RightLanguageButton;
        public Button ResetProgressButton;
        public Image CountryFlagImage;
        public TMPro.TextMeshProUGUI LanguageLabel;
        public Sprite[] LanguageFlags;
    }
}