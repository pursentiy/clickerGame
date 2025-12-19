using System;
using Handlers;
using Installers;
using RSG;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Popup.Base
{
    public abstract class PopupBase<TContext> : InjectableMonoBehaviour where TContext : IPopupContext
    {
        [Inject] protected PopupHandler _popupHandler;
        
        [SerializeField] private AnimationCurve _popupShowAnimationCurve;
        [SerializeField] private AnimationCurve _popupHideAnimationCurve;
        [SerializeField] private RectTransform _popupBaseTransform;
        [SerializeField] private RectTransform _popupOriginTransform;
        [SerializeField] private RectTransform _popupBackgroundTransform;
        [SerializeField] private Image _popupBackgroundImage;
        [SerializeField] private Button _backgroundButton;
        
        private IPopupContext _context;
        
        protected TContext Context => (TContext) _context;

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
        
        public virtual void OnCreated(IPopupContext context)
        {
            _context =  context;
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