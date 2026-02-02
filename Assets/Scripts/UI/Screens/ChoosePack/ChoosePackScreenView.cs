using Components.UI;
using Handlers.UISystem;
using TMPro;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.ChoosePack
{
    public class ChoosePackScreenView : MonoBehaviour, IUIView
    {
        public CurrencyDisplayWidget StarsDisplayWidget;
        public TextMeshProUGUI HeaderText;
        public TextMeshProUGUI AvailablePacksText;
        public PacksWidget PacksWidget;
        public Button GoBackButton;
        public Button SettingsButton;
        public Button InfoButton;
        public Button AdsInfoButton;
        public AdsButtonWidget AdsButton;
        public float InfoMessageFontSize = 150f;
    }
}