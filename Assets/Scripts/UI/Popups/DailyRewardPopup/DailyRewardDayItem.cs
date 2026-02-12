using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardDayItem : MonoBehaviour
    {
        public RectTransform RootTransform;
        public Image RewardIcon;
        public Image LockIcon;
        public TMP_Text LockText;
        public ParticleSystem GlowParticles;
        public CanvasGroup CanvasGroup;
    }
}