using Components.UI;
using Handlers.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupView : MonoBehaviour, IUIView
    {
        public RectTransform MainTransform;
        public Button GoToLevelsChooseScreenButton;
        public Button BackgronudButton;
        public Image[] Stars;
        public Material GrayScaleStarMaterial;
        public Material DefaultStareMaterial;
        public ParticleSystem[] FireworksParticles;
        public TMP_Text TimeText;
        public TMP_Text NewStarText;
        public CanvasGroup NewStarTextCanvasGroup;
        public Color AlreadyEarnedStarColor = new Color(1.0f, 0.83f, 0.74f, 1.0f);
        public CurrencyDisplayWidget CurrencyWidget;
        public RectTransform StarsFlightStartPlace;
        public RectTransform FlyingRewardsContainer;
    }
}