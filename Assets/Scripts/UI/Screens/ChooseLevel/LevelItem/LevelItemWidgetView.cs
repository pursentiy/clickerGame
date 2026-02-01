using ThirdParty.SuperScrollView.Scripts.List;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.ChooseLevel.LevelItem
{
    public class LevelItemWidgetView : ItemViewBase
    {
        public Image LockImage;
        public TMP_Text LevelText;
        public TMP_Text LevelDifficultyText;
        public Image FadeImage;
        public Image LevelImage;
        public Button LevelEnterButton;
        public Image[] StarsImages;
        public Material GrayScaleStarMaterial;
        public Material DefaultStareMaterial;
    }
}