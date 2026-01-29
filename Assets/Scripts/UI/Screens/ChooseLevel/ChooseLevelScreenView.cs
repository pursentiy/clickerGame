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
        public LevelItemWidget _levelItemWidgetPrefab;
        public RectTransform _levelEnterPopupsParentTransform;
        public HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        public Button _goBack;
        public Button _settingsButton;
        public Button _infoButton;
        public CurrencyDisplayWidget _starsDisplayWidget;
        public TextMeshProUGUI _headerText;
        public TextMeshProUGUI _availableLevelsText;
        public TMP_Text _packName;
    }
}