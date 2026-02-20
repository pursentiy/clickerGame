using DG.Tweening;
using UnityEngine;

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

        public void PlayEntrance(float delay, float duration)
        {
            CapturePosition();
            _holder.DOKill();
            
            _holder.DOLocalMove(_initialLocalPos, duration)
                .SetEase(Ease.OutCubic)
                .SetDelay(delay);

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                _canvasGroup.DOFade(1f, duration * 0.8f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(delay);
            }
        }

        public void PlayExit(float offsetY, float duration)
        {
            CapturePosition();
            _holder.DOKill();
            
            _holder.DOLocalMove(_initialLocalPos + new Vector3(0, offsetY, 0), duration)
                .SetEase(Ease.InCubic);

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                _canvasGroup.DOFade(0f, duration)
                    .SetEase(Ease.InQuad);
            }
        }
    }
}