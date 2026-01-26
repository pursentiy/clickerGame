using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups.MessagePopup
{
    public class MessagePopupView : MonoBehaviour, IUIView
    {
        public RectTransform PopupTransform;
        public RectTransform BubbleTransform;
        public RectTransform CornerTransform;
        public Button BackgroundCloseButton;
        public TMP_Text Text;
    }
}