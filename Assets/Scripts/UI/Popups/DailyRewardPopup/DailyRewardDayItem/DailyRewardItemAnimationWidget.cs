using System;
using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public class DailyRewardItemAnimationWidget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform contentHolder;
        [SerializeField] private Canvas itemCanvas;
        [SerializeField] private CanvasGroup itemCanvasGroup;

        private Action _playGlow;
        private Action _playDust;
        private Action _stopGlow;

        [Header("Animation Settings")]
        [SerializeField] private float _readyBounce = 0.1f;

        private int _initialSortingOrder;
        private Vector3 _initialContentScale = Vector3.one;

        private void Awake()
        {
            if (itemCanvas != null)
                _initialSortingOrder = itemCanvas.sortingOrder;
            
            if (contentHolder != null)
                _initialContentScale = contentHolder.localScale;
        }

        public void PrepareForEntrance()
        {
            if (contentHolder != null)
                contentHolder.localScale = Vector3.zero;
            if (itemCanvasGroup != null)
                itemCanvasGroup.alpha = 0f;
        }

        public IPromise PlayEntranceAnimation(float delay, float duration = 0.5f)
        {
            // Сброс начального состояния перед анимацией
            contentHolder.localScale = Vector3.one * 0.85f; // Начинаем с чуть меньшего размера
            contentHolder.anchoredPosition += new Vector2(0, -30f); // Начинаем чуть ниже
            if (itemCanvasGroup != null) itemCanvasGroup.alpha = 0;

            var seq = DOTween.Sequence().KillWith(this).SetDelay(delay);

            seq.Append(DOTween.To(() => itemCanvasGroup.alpha, x => itemCanvasGroup.alpha = x, 1f, duration * 0.6f))
                .Join(contentHolder.DOScale(1f, duration).SetEase(Ease.OutBack, 1.1f))
                .Join(contentHolder.DOAnchorPosY(contentHolder.anchoredPosition.y + 30f, duration).SetEase(Ease.OutCubic));

            return seq.AsPromise().CancelWith(this);
        }

        public IPromise PlayExitAnimation(float duration = 0.3f)
        {
            DOTween.Kill(this);
            
            var seq = DOTween.Sequence().KillWith(this);
            
            seq.Append(contentHolder.DOScale(0.9f, duration).SetEase(Ease.InBack))
                .Join(DOTween.To(() => itemCanvasGroup.alpha, x => itemCanvasGroup.alpha = x, 0f, duration * 0.8f))
                .Join(contentHolder.DOAnchorPosY(contentHolder.anchoredPosition.y - 20f, duration).SetEase(Ease.InCubic));

            return seq.AsPromise().CancelWith(this);
        }

        public void ResetVisuals()
        {
            if (contentHolder == null)
                return;
            contentHolder.DOKill();
            contentHolder.localScale = _initialContentScale;
            contentHolder.localRotation = Quaternion.identity;
            contentHolder.anchoredPosition = Vector2.zero;

            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = false;
                itemCanvas.sortingOrder = _initialSortingOrder;
            }
        }

        public void PlayCurrentDayAnimation()
        {
            StopAnimations();
            if (contentHolder == null)
                return;
            contentHolder.DOScale(_initialContentScale * (1f + _readyBounce), 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

            contentHolder.DORotate(new Vector3(0, 0, 3f), 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

            _playGlow?.Invoke();
        }

        public void SetReadyToCollectCallbacks(Action playGlow, Action playDust, Action stopGlow)
        {
            _playGlow = playGlow;
            _playDust = playDust;
            _stopGlow = stopGlow;
        }

        public void PlayLockedSubtleAnimation()
        {
            if (contentHolder == null)
                return;
            
            contentHolder.DOPunchPosition(new Vector3(2f, 0, 0), 2f, 1, 0.5f)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetId(this);
        }

        public IPromise PlayClaimFeedbackAnimation(Action onClaimComplete)
        {
            var promise = new Promise();
            StopAnimations();

            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = true;
                itemCanvas.sortingOrder = _initialSortingOrder + 100;
            }

            var seq = DOTween.Sequence().KillWith(this);

            if (contentHolder != null)
            {
                seq.Append(contentHolder.DOScale(new Vector3(1.1f, 0.85f, 1f), 0.15f).SetEase(Ease.InOutQuad));
                seq.Append(contentHolder.DOScale(new Vector3(0.9f, 1.2f, 1f), 0.15f).SetEase(Ease.OutQuad));
                seq.Join(contentHolder.DOLocalMoveY(60f, 0.2f).SetRelative().SetEase(Ease.OutCubic));
                seq.Append(contentHolder.DOScale(new Vector3(1.3f, 1.3f, 1f), 0.1f).SetEase(Ease.OutBack));
                seq.Join(contentHolder.DOLocalMoveY(-60f, 0.15f).SetRelative()
                    .SetEase(Ease.InQuart));

                seq.AppendCallback(() =>
                {
                    _playDust?.Invoke();
                    
                    contentHolder.DOShakePosition(0.4f, 10f, 20).KillWith(this);
                    contentHolder.DOShakeRotation(0.4f, 10f, 20).KillWith(this);

                    onClaimComplete?.Invoke();
                });
                seq.Append(contentHolder.DOScale(1f, 0.5f).SetEase(Ease.OutElastic, 0.5f, 0.75f));
            }
            else
            {
                seq.AppendCallback(() => onClaimComplete?.Invoke());
            }

            seq.OnComplete(() =>
            {
                if (itemCanvas != null)
                    itemCanvas.overrideSorting = false;

                promise.SafeResolve();
            });

            return promise;
        }

        public void StopAnimations()
        {
            DOTween.Kill(this);
            if (contentHolder != null)
                contentHolder.DOKill();
            _stopGlow?.Invoke();
        }

        private void OnDisable()
        {
            StopAnimations();
        }
    }
}
