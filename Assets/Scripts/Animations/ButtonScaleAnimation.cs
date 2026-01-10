using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Disposable;

namespace Animations
{
    public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _animationScaleDuration = 0.05f;
        [SerializeField] private float _pointerDownScale = 0.93f;
        [SerializeField] private AnimationCurve _bounceAnimation;

        private Transform _transform;
        private Tween _tween;

        private void Start()
        {
            _transform = _targetTransform;

            if (_transform == null)
            {
                _transform = transform;
            }

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetScaleAnimated(_pointerDownScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetScaleAnimated(1);
        }

        private void SetScaleAnimated(float scale)
        {
            _tween.Kill();

            if (_transform == null)
            {
                return;
            }

            _tween = _bounceAnimation?.keys?.Length > 1 ? _transform.DOScale(scale, _animationScaleDuration).SetEase(_bounceAnimation) : _transform.DOScale(scale, _animationScaleDuration);
            _tween.KillWith(this);
        }
    }
}