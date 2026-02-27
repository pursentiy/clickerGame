using Common.Currency;
using Components.UI;
using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardPopupView : MonoBehaviour, IUIView
    {
        [Header("Root")]
        public RectTransform MainTransform;

        [Header("Texts")]
        public TMP_Text TitleText;
        public TMP_Text DayText;

        [Header("Buttons")]
        public Button CloseButton;
        public Button BackgroundButton;
        public Button InfoButton;

        [Header("Daily Rewards")]
        public DailyRewardDayItem.DailyRewardDayItem[] DayRewardItems;
        public DailyRewardsDaysController DaysController;
        public CurrencyDisplayWidget CurrencyDisplayWidget;
    }
}

