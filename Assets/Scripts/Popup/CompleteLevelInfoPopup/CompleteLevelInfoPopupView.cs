using Components.UI;
using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupView : MonoBehaviour, IUIView
    {
        public RectTransform MainTransform;
        public Button GoToLevelsChooseScreenButton;
        public Button BackgronudButton;
        public Image[] Stars;
        public Material GrayScaleMaterial;
        public ParticleSystem[] FireworksParticles;
        public TMP_Text TimeText;
        public CurrencyDisplayWidget StarsDisplayWidget;
    }
}