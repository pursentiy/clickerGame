using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popup.Settings
{
    public class SettingsPopupView : MonoBehaviour, IUIView
    {
        public Button CloseButton;
        public Button BackgroundButton;
        public Toggle MusicToggle;
        public Toggle SoundToggle;
        public RectTransform MainTransform;
        public Button LeftLanguageButton;
        public Button RightLanguageButton;
        public Image CountryFlagImage;
        public TMP_Text LanguageLabel;
        public Sprite[] LanguageFlags;
        public TMP_SpriteAsset[] LanguageFlagsAssets;
        public Button SaveLanguageButton;
        public RectTransform LanguageChangingContainer;
    }
}