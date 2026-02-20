using ThirdParty.SuperScrollView.Scripts.List;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.ChoosePack.PackLevelItem.Base
{
    public abstract class BasePackItemWidgetView : ItemViewBase
    {
        public RectTransform Holder;
        public Image FadeImage;
        public RectTransform PackImagePrefabContainer;
        public TMP_Text PackText;
        public Button PackEnterButton;
        public TMP_Text LockedBlockText;
        public RectTransform LockedBlockHolder;
        public ParticleSystem UnlockParticles;

        [Header("Unlock Animation Settings")]
        public float UnlockDuration = 0.6f;

        [Header("Entrance Animation (optional)")]
        public CanvasGroup EntranceCanvasGroup;
        public float EntranceSlideOffsetY = 80f;
        public float EntranceDuration = 0.4f;
        public float EntranceStaggerDelay = 0.06f;

        [Header("Exit Animation")]
        public float ExitDuration = 0.25f;

        [Header("Animation (optional)")]
        public PackItemAnimationWidget AnimationWidget;
    }
}
