using System;
using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using Utilities.Disposable;

namespace Common.Widgets.Animations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeWidget : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Ease easeType = Ease.OutCubic;
        
        [Header("Delays")]
        [Tooltip("Задержка ПОСЛЕ того, как объект полностью показан")]
        [SerializeField] private float postShowDelay = 0f;
        [Tooltip("Задержка ПОСЛЕ того, как объект полностью скрыт")]
        [SerializeField] private float postHideDelay = 0.5f;

        [Header("Rich Effect Settings")]
        [SerializeField] private float moveOffset = 30f;
        [SerializeField] private float startAlpha = 0f;

        [Header("References")]
        [SerializeField] private RectTransform targetRectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        private Vector2 _initialAnchoredPosition;
        private bool _isInitialized;

        private void Awake() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            if (targetRectTransform == null) targetRectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            _initialAnchoredPosition = targetRectTransform.anchoredPosition;
            _isInitialized = true;
        }

        public void ResetWidget()
        {
            EnsureInitialized();
            canvasGroup.DOKill();
            targetRectTransform.DOKill();
            
            canvasGroup.alpha = startAlpha;
            canvasGroup.blocksRaycasts = false;
            targetRectTransform.anchoredPosition = _initialAnchoredPosition - new Vector2(0, moveOffset);
        }

        public IPromise Show()
        {
            EnsureInitialized();
            ResetWidget();

            var fadePromise = canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();

            var movePromise = targetRectTransform.DOAnchorPos(_initialAnchoredPosition, duration)
                .SetEase(easeType)
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();

            return Promise.All(fadePromise, movePromise)
                .Then(() => 
                {
                    canvasGroup.blocksRaycasts = true;
                    return Wait(postShowDelay);
                }).CancelWith(this);
        }

        public IPromise Hide()
        {
            EnsureInitialized();
            canvasGroup.DOKill();
            targetRectTransform.DOKill();

            canvasGroup.blocksRaycasts = false;

            var fadePromise = canvasGroup.DOFade(0f, duration * 0.8f)
                .SetEase(Ease.InCubic)
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();

            var movePromise = targetRectTransform.DOAnchorPos(_initialAnchoredPosition - new Vector2(0, moveOffset), duration * 0.8f)
                .SetEase(Ease.InCubic)
                .SetLink(gameObject)
                .AsPromiseWithKillOnCancel();

            return Promise.All(fadePromise, movePromise)
                .Then(() => Wait(postHideDelay)).CancelWith(this);
        }

        /// <summary>
        /// Вспомогательный метод для создания промиса ожидания
        /// </summary>
        private IPromise Wait(float time)
        {
            if (time <= 0) return Promise.Resolved();

            var promise = new Promise();
            DOVirtual.DelayedCall(time, () => promise.Resolve())
                .SetLink(gameObject)
                .SetUpdate(true); // Чтобы задержка работала даже при паузе
            
            return promise;
        }

        [ContextMenu("Test Show")]
        private void TestShow() => Show().Then(() => Debug.Log("Show + Delay Finished"));

        [ContextMenu("Test Hide")]
        private void TestHide() => Hide().Then(() => Debug.Log("Hide + Delay Finished"));
    }
}