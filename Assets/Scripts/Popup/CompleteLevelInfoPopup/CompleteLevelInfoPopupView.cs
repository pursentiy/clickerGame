using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupView : MonoBehaviour, IUIView
    {
        public RectTransform MainTransform;
        public Button CloseButton;
        public Button BackgronudButton;
        public Button RestartLevelButton;
        public Button NextLevelButton;
        public Image[] Stars;
        public Material GrayScaleMaterial;
        public ParticleSystem[] FireworksParticles;
        public TMP_Text TimeText;
    }
}