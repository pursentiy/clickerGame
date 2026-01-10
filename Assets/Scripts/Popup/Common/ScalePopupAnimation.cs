using System;
using DG.Tweening;
using Extensions;
using Handlers.UISystem;
using Installers;
using RSG;
using UnityEngine;
using Zenject;

namespace Popup.Common
{
    public class ScalePopupAnimation : IUIPopupAnimation
    {
        private const float DefaultDuration = 0.2f;
        private const string DefaultAnimShowCurveName = "popup_open";
        private const string DefaultAnimHideCurveName = "InBack";

        //[Inject] private readonly AnimationCurvesLibrary _animationCurvesLibrary;

        private readonly RectTransform _popupRootTransform;

        private readonly float _showDuration;
        private readonly string _easingShowAnimationName;
        private readonly float _hideDuration;
        private readonly string _easingHideAnimationName;

        private readonly float _scale;
        private readonly float _hideScale;

        public ScalePopupAnimation(RectTransform popupRootTransform, float duration = DefaultDuration, float scale = 1,
            float hideScale = 0) :
            this(popupRootTransform, duration, duration, scale, hideScale)
        {
        }

        public ScalePopupAnimation(RectTransform popupRootTransform, float showDuration,
            float hideDuration = DefaultDuration, float scale = 1, float hideScale = 0)
        {
            ContainerHolder.CurrentContainer.Inject(this);

            _popupRootTransform = popupRootTransform;
            _scale = scale;
            _hideScale = hideScale;

            _showDuration = showDuration;
            _hideDuration = hideDuration;
            _easingShowAnimationName = DefaultAnimShowCurveName;
            _easingHideAnimationName = DefaultAnimHideCurveName;
        }

        public void SetShowedState()
        {
            _popupRootTransform.DOKill();
        }

        public void SetHiddenState()
        {
        }

        public IPromise AnimateShow(object context)
        {
            var promise = new Promise();
            _popupRootTransform.localScale = Vector3.one * _hideScale;

            if (_easingShowAnimationName == DefaultAnimShowCurveName)
            {
                //TODO ADD Serialized Dictionary Lite
                //_popupRootTransform.DOScale(_scale, _showDuration).SetEase(_animationCurvesLibrary.GetCurve(DefaultAnimShowCurveName)).OnComplete(promise.Resolve);
                
                _popupRootTransform.DOScale(_scale, _showDuration).SetEase(Ease.InQuad).OnComplete(() => promise.SafeResolve());
            }
            else
            {
                Enum.TryParse(_easingShowAnimationName, out Ease type);
                _popupRootTransform.DOScale(_scale, _showDuration).SetEase(type).OnComplete(promise.Resolve);
            }

            return promise;
        }

        public IPromise AnimateHide(object context)
        {
            var promise = new Promise();
            _popupRootTransform.localScale = Vector3.one * _scale;
            Enum.TryParse(_easingHideAnimationName, out Ease type);
            _popupRootTransform.DOScale(_hideScale, _hideDuration).SetEase(type).OnComplete(promise.Resolve);
            return promise;
        }
    }
}