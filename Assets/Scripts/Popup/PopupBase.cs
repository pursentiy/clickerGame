using System;
using Handlers;
using Installers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Popup
{
    public class PopupBase : InjectableMonoBehaviour
    {
        [Inject] protected PopupHandler _popupHandler;
        
        [SerializeField] private AnimationCurve _popupShowAnimationCurve;
        [SerializeField] private AnimationCurve _popupHideAnimationCurve;
        [SerializeField] private RectTransform _popupBaseTransform;
        [SerializeField] private RectTransform _popupOriginTransform;
        [SerializeField] private RectTransform _popupBackgroundTransform;
        [SerializeField] private Image _popupBackgroundImage;
        [SerializeField] private Button _backgroundButton;

        public AnimationCurve PopupShowAnimationCurve => _popupShowAnimationCurve;
        public AnimationCurve PopupHideAnimationCurve => _popupHideAnimationCurve;
        public RectTransform PopupBaseTransform => _popupBaseTransform;
        public RectTransform PopupBackgroundTransform => _popupBackgroundTransform;
        public RectTransform PopupOriginTransform => _popupOriginTransform;
        public Image PopupBackgroundImage => _popupBackgroundImage;
        public Button BackgroundButton => _backgroundButton;

        private void Start()
        {
            OnCreated();
        }

        protected virtual void OnCreated()
        {
            _backgroundButton.onClick.AddListener(()=> _popupHandler.HideCurrentPopup());
        }

        private void OnDestroy()
        {
            _backgroundButton.onClick.RemoveAllListeners();
        }
    }
}