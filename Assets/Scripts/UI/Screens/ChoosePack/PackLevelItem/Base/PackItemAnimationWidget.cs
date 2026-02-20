using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Screens.ChoosePack.PackLevelItem.Base
{
    public class PackItemAnimationWidget : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _holder;
    
        private Vector3 _initialLocalPos;
        private bool _isPosCaptured;

        // Важно вызвать это ДО любых манипуляций, чтобы запомнить "нулевую" точку
        private void CapturePosition()
        {
            if (_isPosCaptured) 
                return;
            _initialLocalPos = _holder.localPosition;
            _isPosCaptured = true;
        }

        /// <summary>
        /// Сбрасывает захваченную позицию, чтобы следующий CapturePosition взял актуальную.
        /// </summary>
        public void ResetPositionCapture()
        {
            _isPosCaptured = false;
        }

        public void Prepare(float offsetY)
        {
            CapturePosition();
            _holder.localPosition = _initialLocalPos + new Vector3(0, offsetY, 0);
            if (_canvasGroup != null) _canvasGroup.alpha = 0;
        }
        
        public void SetToRest()
        {
            CapturePosition();
            _holder.DOKill();
            _holder.localPosition = _initialLocalPos;
            if (_canvasGroup != null) 
                _canvasGroup.alpha = 1f;
        }

        public IPromise PlayEntrance(float delay, float duration)
        {
            CapturePosition();
            _holder.DOKill();
            
            var canvasPromise = Promise.Resolved();
            var holderAnimationPromise = _holder.DOLocalMove(_initialLocalPos, duration)
                .SetEase(Ease.OutCubic)
                .SetDelay(delay)
                .KillWith(this)
                .AsPromise();

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                canvasPromise = _canvasGroup.DOFade(1f, duration * 0.8f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(delay)
                    .KillWith(this)
                    .AsPromise();
            }

            return Promise.All(canvasPromise, holderAnimationPromise);
        }

        public IPromise PlayExit(float offsetY, float duration)
        {
            CapturePosition();
            _holder.DOKill();
            
            var canvasPromise = Promise.Resolved();
            var holderAnimationPromise = 
                _holder.DOLocalMove(_initialLocalPos + new Vector3(0, offsetY, 0), duration)
                .SetEase(Ease.InCubic)
                .KillWith(this)
                .AsPromise();

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                canvasPromise = _canvasGroup.DOFade(0f, duration)
                    .SetEase(Ease.InQuad)
                    .KillWith(this)
                    .AsPromise();
            }
            
            return Promise.All(canvasPromise, holderAnimationPromise);
        }
    }
}