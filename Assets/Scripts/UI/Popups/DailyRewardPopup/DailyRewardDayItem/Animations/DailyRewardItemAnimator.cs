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
            _ctx.ContentHolder.localRotation = Quaternion.Euler(0, 0, -10f); // Легкий наклон
    
            if (_ctx.ItemCanvasGroup != null) _ctx.ItemCanvasGroup.alpha = 0;

            var seq = DOTween.Sequence()
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
            Stop();
            _ctx.ContentHolder.DOScale(_ctx.InitialScale * (1f + _ctx.ReadyBounce), 1.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).KillWith(_ctx.DisposeProvider);
            _ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y + 15f, 2f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).KillWith(_ctx.DisposeProvider);
            _ctx.PlayGlow();
        }

        public void PlayLockedSubtle()
        {
            Stop();
    
            // Создаем Sequence для сложной, "дорогой" анимации покоя
            var seq = DOTween.Sequence()
                .KillWith(_ctx.DisposeProvider)
                .SetLoops(-1, LoopType.Yoyo);
            
            seq.Append(_ctx.ContentHolder.DOScale(_ctx.InitialScale * 0.97f, 3f).SetEase(Ease.InOutSine));
            seq.Join(_ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y - 5f, 3f).SetEase(Ease.InOutSine));
            if (_ctx.ItemCanvasGroup != null)
            {
                seq.Join(_ctx.ItemCanvasGroup.DOFade(0.85f, 3f).SetEase(Ease.InOutSine));
            }
        }
        public IPromise PlayClaim(Action onMidPoint)
        {
            Stop();
            if (_ctx.ItemCanvas != null)
            {
                _ctx.ItemCanvas.overrideSorting = true;
                _ctx.ItemCanvas.sortingOrder = _ctx.InitialSortingOrder + 100;
            }

            var seq = DOTween.Sequence().KillWith(_ctx.DisposeProvider);
            seq.Append(_ctx.ContentHolder.DOScale(new Vector3(1.15f, 0.8f, 1f), 0.12f).SetEase(Ease.OutQuad))
               .Append(_ctx.ContentHolder.DOScale(new Vector3(0.85f, 1.25f, 1f), 0.15f).SetEase(Ease.OutQuad))
               .Join(_ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y + 80f, 0.2f).SetEase(Ease.OutCubic))
               .Append(_ctx.ContentHolder.DOScale(_ctx.InitialScale * 1.25f, 0.1f).SetEase(Ease.InQuad))
               .Join(_ctx.ContentHolder.DOAnchorPos(_ctx.InitialPos, 0.1f).SetEase(Ease.InQuad))
               .AppendCallback(() => {
                   _ctx.PlayDust();
                   _ctx.ContentHolder.DOShakeRotation(0.5f, 15f).KillWith(_ctx.DisposeProvider);
                   onMidPoint?.Invoke();
               })
               .Append(_ctx.ContentHolder.DOScale(_ctx.InitialScale, 0.6f).SetEase(Ease.OutElastic, 0.4f, 0.6f))
               .OnComplete(() => {
                   DOVirtual.DelayedCall(0.1f, () => { if (_ctx.ItemCanvas != null) _ctx.ItemCanvas.overrideSorting = false; }).KillWith(_ctx.DisposeProvider);
               });
            return seq.AsPromise().CancelWith(_ctx.DisposeProvider);
        }

        public IPromise PlayExit(float duration = 0.3f)
        {
            Stop();
            return DOTween.Sequence().KillWith(_ctx.DisposeProvider)
                .Append(_ctx.ContentHolder.DOScale(_ctx.InitialScale * 0.8f, duration).SetEase(Ease.InBack))
                .Join(_ctx.ItemCanvasGroup.DOFade(0f, duration * 0.8f))
                .Join(_ctx.ContentHolder.DOAnchorPosY(_ctx.InitialPos.y - 20f, duration).SetEase(Ease.InCubic))
                .AsPromise();
        }
        
        public void PlayCollectedSubtle()
        {
            Stop();
            
            _ctx.ContentHolder.DOLocalRotate(new Vector3(0, 0, 1.2f), 3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(_ctx);
        }

        public void Dispose() => Stop();
        
        [UsedImplicitly]
        public class Factory : PlaceholderFactory<IDailyRewardAnimationContext, DailyRewardItemAnimator> { }
    }
}