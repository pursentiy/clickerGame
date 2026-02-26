using System;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using RSG;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem.Animations
{
    public class DailyRewardItemAnimator : IDisposable
    {
        private readonly IDailyRewardAnimationContext _ctx;

        public DailyRewardItemAnimator(IDailyRewardAnimationContext context) => _ctx = context;

        public void Stop() => DOTween.Kill(_ctx);

        public void Reset()
        {
            Stop();
            if (_ctx.ContentHolder == null) return;
            
            _ctx.ContentHolder.localScale = _ctx.InitialScale;
            _ctx.ContentHolder.anchoredPosition = _ctx.InitialPos;
            _ctx.ContentHolder.localRotation = Quaternion.identity;
            
            if (_ctx.ItemCanvasGroup != null) _ctx.ItemCanvasGroup.alpha = 1f;
            if (_ctx.ItemCanvas != null) _ctx.ItemCanvas.overrideSorting = false;
        }
        
        public void SetupInvisibleState()
        {
            Stop();
            if (_ctx.ContentHolder != null) 
                _ctx.ContentHolder.localScale = Vector3.zero;
    
            if (_ctx.ItemCanvasGroup != null) 
                _ctx.ItemCanvasGroup.alpha = 0f;
        }

        public IPromise PlayEntrance(float delay)
        {
            Stop(); 
            
            _ctx.ContentHolder.localScale = _ctx.InitialScale * 0.4f; 
            _ctx.ContentHolder.anchoredPosition = _ctx.InitialPos + new Vector2(0, -100f);
            _ctx.ContentHolder.localRotation = Quaternion.Euler(0, 0, -10f);
    
            if (_ctx.ItemCanvasGroup != null) _ctx.ItemCanvasGroup.alpha = 0;

            var seq = DOTween.Sequence()
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider)
                .SetDelay(delay);
            
            seq.Append(_ctx.ItemCanvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic));
            seq.Join(_ctx.ContentHolder.DOAnchorPos(_ctx.InitialPos, 0.6f).SetEase(Ease.OutQuint));
            seq.Join(_ctx.ContentHolder.DOScale(_ctx.InitialScale, 0.75f).SetEase(Ease.OutBack, 1.5f));
            seq.Join(_ctx.ContentHolder.DOLocalRotate(Vector3.zero, 0.8f).SetEase(Ease.OutElastic, 0.6f, 0.4f));

            return seq.AsPromise().CancelWith(_ctx.DisposeProvider);
        }

        public void PlayReadyLoop()
        {
            Reset(); // ГАРАНТИЯ, что анимация начнется из ровного положения, а не вкривь
            
            _ctx.ContentHolder.DOScale(_ctx.InitialScale * 1.05f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider);
        
            // Используем абсолютный сдвиг от InitialPos, чтобы не ломать сетку
            _ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y + 12f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider);

            _ctx.PlayGlow();
        }

        public void PlayLockedSubtle()
        {
            Reset(); // ГАРАНТИЯ ровного старта
            
            _ctx.ContentHolder.DOScale(_ctx.InitialScale * 0.96f, 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider);

            if (_ctx.ItemCanvasGroup != null)
            {
                _ctx.ItemCanvasGroup.DOFade(0.8f, 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                    .KillWith(_ctx.DisposeProvider);
            }
        }
        
        public IPromise PlayClaim(Action onMidPoint)
        {
            Reset(); // Карточка должна стоять ровно перед прыжком
            
            if (_ctx.ItemCanvas != null)
            {
                _ctx.ItemCanvas.overrideSorting = true;
                _ctx.ItemCanvas.sortingOrder = _ctx.InitialSortingOrder + 100;
            }

            var seq = DOTween.Sequence()
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider);

            // Взлет вверх от изначальной позиции
            seq.Append(_ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y + 120f, 0.5f).SetEase(Ease.OutCubic))
                .Join(_ctx.ContentHolder.DOScale(_ctx.InitialScale * 1.35f, 0.5f).SetEase(Ease.OutBack))
                .Join(_ctx.ContentHolder.DOLocalRotate(new Vector3(0, 0, 6f), 0.5f).SetEase(Ease.OutSine));

            seq.AppendInterval(0.05f);

            // Падение ровно в InitialPos
            seq.Append(_ctx.ContentHolder.DOAnchorPos(_ctx.InitialPos, 0.15f).SetEase(Ease.InQuint))
                .Join(_ctx.ContentHolder.DOScale(_ctx.InitialScale, 0.15f).SetEase(Ease.InQuint))
                .Join(_ctx.ContentHolder.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.InQuint));

            seq.AppendCallback(() => {
                _ctx.PlayDust();
                _ctx.ContentHolder.DOShakePosition(0.4f, 15f, 20).SetId(_ctx).KillWith(_ctx.DisposeProvider);
                onMidPoint?.Invoke();
            });

            seq.Append(_ctx.ContentHolder.DOPunchScale(new Vector3(0.15f, -0.15f, 0), 0.5f, 10, 1f).SetId(_ctx));

            seq.OnComplete(() => {
                if (_ctx.ItemCanvas != null) _ctx.ItemCanvas.overrideSorting = false;
            });

            return seq.AsPromise().CancelWith(_ctx.DisposeProvider);
        }

        public IPromise PlayExit(float duration = 0.3f)
        {
            Stop();
            return DOTween.Sequence()
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider)
                .Append(_ctx.ContentHolder.DOScale(_ctx.InitialScale * 0.8f, duration).SetEase(Ease.InBack))
                .Join(_ctx.ItemCanvasGroup.DOFade(0f, duration * 0.8f))
                .Join(_ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y - 20f, duration).SetEase(Ease.InCubic))
                .AsPromise();
        }
        
        public void PlayCollectedSubtle()
        {
            Reset(); // ГАРАНТИЯ
            
            _ctx.ContentHolder.DOLocalRotate(new Vector3(0, 0, 1.2f), 3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(_ctx) // ОБЯЗАТЕЛЬНО
                .KillWith(_ctx.DisposeProvider);
        }

        public void Dispose() => Stop();
        
        [UsedImplicitly]
        public class Factory : PlaceholderFactory<IDailyRewardAnimationContext, DailyRewardItemAnimator> { }
    }
}