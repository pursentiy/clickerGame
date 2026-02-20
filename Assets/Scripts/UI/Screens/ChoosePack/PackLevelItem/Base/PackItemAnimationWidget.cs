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
    
            // Создаем последовательность для "дорогого" эффекта
            var seq = DOTween.Sequence().SetDelay(delay).SetTarget(this);

            // 1. Плавный вылет с небольшим перехлестом (Back)
            seq.Append(_holder.DOLocalMove(_initialLocalPos, duration)
                .SetEase(Ease.OutBack, 1.2f)); // 1.2f - коэффициент "прыгучести"

            // 2. Squash & Stretch при приземлении (эффект живого объекта)
            // Объект чуть сплющивается в конце движения
            seq.Join(_holder.DOScaleX(1.05f, duration * 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));
            seq.Join(_holder.DOScaleY(0.95f, duration * 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));

            // 3. Плавное появление альфы
            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                _canvasGroup.alpha = 0;
                seq.Join(_canvasGroup.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));
            }

            return seq.KillWith(this).AsPromise();
        }

        public IPromise PlayExit(float offsetY, float duration)
        {
            CapturePosition();
            _holder.DOKill();
            if (_canvasGroup != null) _canvasGroup.DOKill();

            // Создаем Sequence, чтобы объединить подготовку и вылет
            var seq = DOTween.Sequence().SetTarget(this);

            // 1. Anticipation: Объект слегка "приседает" вниз и расширяется перед прыжком
            // Это добавляет физического веса
            float anticipationTime = duration * 0.25f;
            seq.Append(_holder.DOBlendableLocalMoveBy(new Vector3(0, -20f, 0), anticipationTime)
                .SetEase(Ease.OutQuad));
            seq.Join(_holder.DOScaleX(1.1f, anticipationTime).SetEase(Ease.OutQuad));
            seq.Join(_holder.DOScaleY(0.9f, anticipationTime).SetEase(Ease.OutQuad));

            // 2. Launch: Резкий выстрел вверх
            // Используем InBack для эффекта ускорения в конце
            float launchTime = duration * 0.75f;
            seq.Append(_holder.DOLocalMove(_initialLocalPos + new Vector3(0, offsetY, 0), launchTime)
                .SetEase(Ease.InBack, 1.5f));

            // 3. Stretch: Объект вытягивается по вертикали во время полета
            seq.Join(_holder.DOScaleX(0.85f, launchTime).SetEase(Ease.InSine));
            seq.Join(_holder.DOScaleY(1.2f, launchTime).SetEase(Ease.InSine));

            // 4. Fade: Исчезновение начинается чуть позже старта прыжка
            if (_canvasGroup != null)
            {
                seq.Join(_canvasGroup.DOFade(0f, launchTime).SetEase(Ease.InQuad));
            }

            // Возвращаем как Promise для синхронизации с другими элементами
            return seq.KillWith(this).AsPromise();
        }
    }
}