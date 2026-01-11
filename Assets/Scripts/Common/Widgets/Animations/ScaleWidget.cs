using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;

namespace Common.Widgets.Animations
{
    public class ScaleWidget : MonoBehaviour
    {
        [Header("Animation Settings")] [Tooltip("Duration of the scale animation")]
        [SerializeField] private float duration = 0.5f;

        [Tooltip("Amount of overshoot for the spring effect (0 for no overshoot)")]
        [SerializeField] private float overshoot = 0.8f;

        [Tooltip("Ease type for the animation (e.g., Ease.OutBack for springy feel)")]
        [SerializeField] private Ease easeType = Ease.OutBack;

        [Header("References")] [Tooltip("The parent GameObject of your block (image and text)")]
        [SerializeField] private RectTransform targetRectTransform; // Используем RectTransform для UI

        [Tooltip("CanvasGroup on the parent for fade effect (optional)")]
        [SerializeField] private CanvasGroup canvasGroup; // Для плавного появления/исчезновения
        
        [SerializeField] private Vector3 targetScale = Vector3.one;

        public void ResetWidget()
        {
            targetRectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Запускает анимацию появления объекта.
        /// </summary>
        public IPromise Show()
        {
            targetRectTransform.DOKill(true);
            canvasGroup.DOKill(true);
            
            var scalePromise = targetRectTransform.DOScale(targetScale, duration)
                .SetEase(easeType, overshoot) // Используем overshoot для пружинистого эффекта
                .SetLink(gameObject) // Привязываем к жизненному циклу GameObject
                .OnComplete(() => canvasGroup.blocksRaycasts = true)
                .AsPromiseWithKillOnCancel(); // Включаем взаимодействие после появления
            
            var fadePromise = canvasGroup.DOFade(1f, duration * 0.5f) // Делаем Fade немного быстрее, чтобы выглядело приятнее
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();

            var promise = Promise.All(scalePromise, fadePromise);
            
            promise.Finally(() =>
            {
                if (canvasGroup != null && canvasGroup.gameObject != null)
                    canvasGroup.blocksRaycasts = true;
            });
            
            return promise;
        }
        
        public IPromise Hide()
        {
            targetRectTransform.DOKill(true);
            canvasGroup.DOKill(true);

            canvasGroup.blocksRaycasts = false; // Отключаем взаимодействие сразу
            
            var scalePromise = targetRectTransform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack) // Для исчезновения часто подходит Ease.InBack
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();
            
            var fadePromise = canvasGroup.DOFade(0f, duration * 0.5f)
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();
            
            return Promise.All(scalePromise, fadePromise);
        }

        // Пример использования (можно вызывать из кнопки или другого скрипта)
        [ContextMenu("Test Show")]
        private void TestShow() => Show();

        [ContextMenu("Test Hide")]
        private void TestHide() => Hide();
    }
}