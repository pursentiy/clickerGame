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
        public PackItemWidget PackItemWidgetPrefab;
        public RectTransform LevelEnterPopupsParentTransform;
        public HorizontalLayoutGroup HorizontalLayoutGroupPrefab;
        public CurrencyDisplayWidget StarsDisplayWidget;
        public TextMeshProUGUI HeaderText;
        public TextMeshProUGUI AvailablePacksText;
        public Button GoBack;
        public Button SettingsButton;
        public Button InfoButton;
        public AdsButtonWidget AdsButton;
        [Range(1, 5)]
        public int RowPacksCount = 2;
    }
}