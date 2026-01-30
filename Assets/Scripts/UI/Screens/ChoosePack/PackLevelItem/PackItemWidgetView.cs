using ThirdParty.SuperScrollView.Scripts.List;
using ThirdParty.SuperScrollView.Scripts.ListView;
using TMPro;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.ChoosePack.PackLevelItem
{
    public class PackItemWidgetView : ItemViewBase
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
    }
}