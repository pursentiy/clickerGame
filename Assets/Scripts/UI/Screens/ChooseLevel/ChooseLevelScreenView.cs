using Components.UI;
using Handlers.UISystem;
using TMPro;
using UI.Screens.ChooseLevel.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.ChooseLevel
{
    public class ChooseLevelScreenView : MonoBehaviour, IUIView
    {
        public LevelsWidget LevelsWidget;
        public Button GoBackButton;
        public Button SettingsButton;
        public Button InfoButton;
        public CurrencyDisplayWidget StarsDisplayWidget;
        public TextMeshProUGUI HeaderText;
        public TextMeshProUGUI AvailableLevelsText;
        public TMP_Text PackName;
        public float InfoMessageFontSize = 150f;
    }
}