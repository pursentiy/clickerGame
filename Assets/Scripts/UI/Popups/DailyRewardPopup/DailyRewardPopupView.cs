using Common.Currency;
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
        public Button ClaimRewardsButton;
        public Button CloseButton;
        public Button InfoButton;

        [Header("Current Reward Panel")]
        public RectTransform CurrentRewardPanel;
        public RectTransform CurrentRewardContainer;

        [Header("Next Reward Panel")]
        public RectTransform NextRewardPanel;
        public RectTransform NextRewardContainer;

        [Header("Reward Item Prefab")]
        public AssetReference RewardItemPrefab;

        [Header("7 Day Rewards")]
        public DailyRewardDayItem[] DayRewardItems = new DailyRewardDayItem[7];
    }

    [System.Serializable]
    public class DailyRewardDayItem
    {
        public RectTransform RootTransform;
        public Image RewardIcon;
        public Image LockIcon;
        public TMP_Text LockText;
        public ParticleSystem GlowParticles;
        public CanvasGroup CanvasGroup;
    }
}

