using System;
using System.Linq;
using DG.Tweening;
using Extensions;
using RSG;
using Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities.Disposable;
using Random = UnityEngine.Random;

namespace Common.Rewards
{
    public class RewardAnimation : RewardAnimationBase
    {
        [SerializeField] private RectTransform _itemPrefab;
        [SerializeField] private int _poolCount = 15;
        [SerializeField] private float _particlesScale = 1;
        
        public override int PoolCount => _poolCount;
        
        public void SetRewardsSprite(Sprite sprite)
        {
            if (ItemsPool.IsNullOrEmpty())
                return;

            foreach (var item in ItemsPool)
            {
                var image = item.GetComponentInChildren<Image>();
                    
                    if (image != null)
                        image.sprite = sprite;
            }
        }
        
        protected override (IPromise AllParticlesFinish, IPromise AnyParticleFinish) PerformAnimation(
            Vector2 startingPos,
            Vector2 targetPos,
            float? maybeParticleSize,
            long count,
            float moveTimeSpeedupFactor,
            RectTransform targetContainer = null, 
            Transform endWidget = null,
            Transform endWidgetParent = null,
            float appearDistanceScale = 1f,
            UnityAction callbackForEachAnimation = null,
            float delaySpeedupFactor = 1f
        )
        {
            if (ThisRT == null)
            {
                ThisRT = this.transform;
            }

            if (ItemsPool == null)
            {
                LoggerService.LogWarning($"Cannot play FlyingUIRewardAnimation. _coinsPool parameter is null");
                return new ValueTuple<IPromise, IPromise>(Promise.Resolved(), Promise.Resolved());
            }

            var longCount = count;
            if (longCount > _poolCount)
                longCount = _poolCount;

            if (longCount < 1)
                longCount = 1;

            var elementsCount = (int) longCount;
            
            Sequence endWidgetParentTween = null;
            Sequence endWidgetTween = null;

            var index = 0;

            // TEMP NRE wrapper fix for https://bitbucket.org/casualgamesonline/cm-client/pull-requests/664/puzzle-fix/diff
            var promises = ItemsPool.Take(elementsCount).Map(item =>
            {
                var itemTransform = item.GetRectTransform();
                if (itemTransform == null)
                    return Promise.Resolved();

                itemTransform.anchoredPosition = Vector2.zero;
                itemTransform.SetUniversalScale(0);
                itemTransform.TrySetActive(true);
                itemTransform.DOKill();

                var thisPos = (Vector2) itemTransform.position ;
                var finalPosition = new Vector3((targetPos - startingPos).x, (targetPos - startingPos).y, 0);
                
                var prepPosition = Vector2.one.Rotate(Random.Range(0f, 360f)) * _appearDistance.GetRandomValue();
                var startPosition = prepPosition * appearDistanceScale + thisPos;
                var middlePosition = ((targetPos - thisPos) / 2f).Rotate(Random.Range(-_midPointCoeff, _midPointCoeff));
                var path = new Vector3[]
                {
                    new (startPosition.x, startPosition.y),
                    middlePosition + thisPos,
                    targetPos
                };

                var particleScale = _particlesScale * (maybeParticleSize.HasValue ? maybeParticleSize.Value : 1f);

                //TODO MAYBE UNCOMMENT LATER
                // if (targetContainer != null && _canvas != null)
                // {
                //     targetContainer.EnableCanvasOverriding();
                //
                //     _canvas.overrideSorting = true;
                //     _canvas.sortingOrder = 113;
                // }

                var moveDuration = MoveTime / (2 * moveTimeSpeedupFactor);
                var intervalRandomValue = index * moveDuration * DelayCoef;
                var hoveringTween = DOTween.Sequence();

                var tween = DOTween.Sequence()
                    .AppendInterval(intervalRandomValue)
                    .Append(itemTransform.DOMove(startPosition, moveDuration * _stagesCoef).SetEase(_moveInCurve))
                    .Join(itemTransform.DOScale(particleScale, moveDuration * _stagesCoef).SetEase(_scaleCurve));

                tween
                    .Insert(DelayAfterAdditionalAnimation * delaySpeedupFactor + index * FinalStageDelayCoef, itemTransform.DOScale(particleScale * 0.6f, moveDuration * (1 - _stagesCoef)).SetEase(Ease.InOutQuad))
                    .Join(itemTransform.DOPath(path, moveDuration * (1 - _stagesCoef), PathType.CatmullRom).SetEase(Ease.InCubic))
                    .Append(itemTransform.DOScale(0, 0.15f).SetEase(Ease.InOutQuad))
                    .InsertCallback(0.8f * (DelayAfterAdditionalAnimation * delaySpeedupFactor + intervalRandomValue + moveDuration + index * FinalStageDelayCoef), () =>
                    {
                        if (endWidgetParent != null && endWidgetParentTween == null)
                        {
                            var endWidgetParentStartLocalScale = endWidgetParent.localScale;

                            endWidgetParentTween = DOTween.Sequence()
                                .Append(endWidgetParent.DOScale(1.15f, 0.07f).SetLoops(elementsCount, LoopType.Yoyo).SetEase(Ease.InOutQuad))
                                .OnComplete(() => endWidgetParent.localScale = endWidgetParentStartLocalScale)
                                .KillWith(this);
                        }

                        if (endWidget != null && endWidgetTween == null)
                        {
                            var endWidgetStartLocalScale = endWidget.localScale;

                            endWidgetTween = DOTween.Sequence()
                                .Append(endWidget.DOScale(1.25f, 0.07f).SetLoops(elementsCount, LoopType.Yoyo).SetEase(Ease.InOutQuad))
                                .OnComplete(() => endWidget.localScale = endWidgetStartLocalScale).KillWith(this);
                        }

                        if (targetContainer != null)
                        {
                            DOTween.Sequence()
                                .Append(targetContainer.DOScale(1.18f, 0.15f * moveDuration * path.Length).SetEase(Ease.InOutQuad))
                                .Append(targetContainer.DOScale(1f, 0.075f * moveDuration * path.Length).SetEase(Ease.InOutQuad))
                                .AppendInterval(0.25f)
                                .KillWith(this);
                        }
                    })
                    .AppendCallback(() =>
                    {
                        hoveringTween?.Kill();
                        item.TrySetActive(false);
                        if (targetContainer != null && _canvas != null)
                            _canvas.overrideSorting = false;
                    }).KillWith(this);

                var tweenPromise = tween.AsPromiseWithKillOnCancel(true);
                tweenPromise
                    .Then(() => callbackForEachAnimation?.Invoke())
                    .OnCancel(() => 
                    {
                        hoveringTween?.Kill();
                        endWidgetParentTween?.Kill(true);
                        endWidgetTween?.Kill(true);
                        tween.Kill(true);
                    });

                index++;
                return tweenPromise;
            });
            
            var allPromise = Promise.All(promises)
                .ContinueWithResolved(() =>
                {
                    endWidgetParentTween?.Kill(true);
                    endWidgetTween?.Kill(true);
                });

            return new ValueTuple<IPromise, IPromise>(allPromise, Promise.Race(promises));
        }
        
        private void Awake()
        {
            CreateCoinsPool(_itemPrefab.gameObject);
        }
    }
}