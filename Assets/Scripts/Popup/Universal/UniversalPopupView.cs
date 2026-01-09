using System;
using Handlers.UISystem;
using Plugins.RotaryHeart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popup.Universal
{
    public class UniversalPopupView : MonoBehaviour, IUIView
    {
        public CanvasGroup MainGroup;
        public TextMeshProUGUI MessageText;
        public RectTransform MessageBackgroundTransform;
        public Button CommonCrossButton;
        public Button BackgroundButton;
        public GameObject TitleBar;
        public TextMeshProUGUI Title;
        public RectTransform ButtonsContainer;

        public Button ButtonDefaultPrefab;
        public UniversalPopupButtonStylesDictionary ButtonStyles;
    }

    [Serializable]
    public class UniversalPopupButtonStylesDictionary : UnityDictionary<UniversalPopupButtonStyle, Button>
    {
        
    }
}