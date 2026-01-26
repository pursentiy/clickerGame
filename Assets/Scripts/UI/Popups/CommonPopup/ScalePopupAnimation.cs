using System;
using DG.Tweening;
using Extensions;
using Handlers.UISystem;
using Installers;
using RSG;
using Services;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Popups.CommonPopup
{
    public class ScalePopupAnimation : IUIPopupAnimation
    {
        private const float DefaultDuration = 0.2f;
        private const string DefaultAnimShowCurveName = "popup_open";
        private const string DefaultAnimHideCurveName = "InBack";

        //[Inject] private readonly AnimationCurvesLibrary _animationCurvesLibrary;

        private readonly RectTransform _popupRootTransform;

        private readonly float _showDuration;
        private readonly float _hideDuration;

        private readonly float _scale;
        private readonly float _hideScale;
        private readonly float _moveOffset = 10f;

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
            
            _popupRootTransform.DOKill();
            _popupRootTransform.localScale = Vector3.one * _hideScale;
            var localPosition = _popupRootTransform.localPosition;
            _popupRootTransform.localPosition = new Vector2(localPosition.x, localPosition.y - _moveOffset);

            Sequence showSequence = DOTween.Sequence().KillWith(_popupRootTransform.gameObject);

            showSequence.Append(_popupRootTransform.DOScale(_scale, _showDuration).SetEase(Ease.OutBack, 1.2f));
            showSequence.Join(_popupRootTransform.DOLocalMove(localPosition, _showDuration).SetEase(Ease.OutCubic));
            
            showSequence.OnComplete(() => promise.SafeResolve());

            return promise;
        }

        public IPromise AnimateHide(object context)
        {
            if (_popupRootTransform == null || _popupRootTransform.gameObject == null) 
                return Promise.Resolved();
            
            var promise = new Promise();

            _popupRootTransform.DOKill();

            var hideSequence = DOTween.Sequence().KillWith(_popupRootTransform.gameObject);

            var localPosition = _popupRootTransform.localPosition;
            hideSequence.Append(_popupRootTransform.DOScale(_hideScale, _hideDuration).SetEase(Ease.InBack));
            hideSequence.Join(_popupRootTransform.DOLocalMove(new Vector2(localPosition.x, localPosition.y - _moveOffset), _hideDuration).SetEase(Ease.InQuad));

            hideSequence.OnComplete(() => promise.SafeResolve());

            return promise;
        }
    }
}