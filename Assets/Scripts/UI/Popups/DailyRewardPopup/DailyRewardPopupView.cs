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
        public RectTransform FlyingRewardsContainer;

        [Header("Texts")]
        public TMP_Text TitleText;
        public TMP_Text DayText;

        [Header("Buttons")]
        public Button ClaimRewardsButton;
        public Button CloseButton;
        public Button InfoButton;
        public CurrencyDisplayWidget CurrencyDisplayWidget;

        [Header("Daily Rewards")]
        public DailyRewardDayItem[] DayRewardItems;
    }
}

