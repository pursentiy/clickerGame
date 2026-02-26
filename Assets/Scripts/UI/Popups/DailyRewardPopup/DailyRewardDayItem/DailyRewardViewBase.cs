using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public abstract class DailyRewardViewBase : MonoBehaviour
    {
        [SerializeField] protected Image rewardIcon;
        [SerializeField] protected TMP_Text rewardCurrencyText;
        [SerializeField] protected TMP_Text infoText;

        public virtual void SetRewardIcon(Sprite icon)
        {
            rewardIcon.sprite = icon;
        }

        public virtual void SetRewardText(string text)
        {
            rewardCurrencyText.text = text ?? string.Empty;
        }

        public virtual void SetInfoText(string text)
        {
            infoText.text = text ?? string.Empty;
        }

        public virtual void ApplyVisuals(DayItemState state)
        {
            var grayed = state == DayItemState.MissedDay;
            rewardIcon.color = grayed ? new Color(0.4f, 0.4f, 0.4f, 1f) : Color.white;
            rewardCurrencyText.gameObject.SetActive(true);
        }
    }
}
