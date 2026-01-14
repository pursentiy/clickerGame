using System;
using System.Linq;
using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities.Disposable;

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
            if (ThisRT == null) ThisRT = this.transform;
            if (ItemsPool == null) return new ValueTuple<IPromise, IPromise>(Promise.Resolved(), Promise.Resolved());

            var elementsCount = (int)Mathf.Clamp(count, 1, _poolCount);

            var index = 0;
            var promises = ItemsPool.Take(elementsCount).Map((item) =>
            {
                var itemTransform = item.GetRectTransform();
                if (itemTransform == null) return Promise.Resolved();

                // --- Initial Reset ---
                itemTransform.position = startingPos;
                itemTransform.localScale = Vector3.zero;
                itemTransform.TrySetActive(true);
                itemTransform.DOKill();

                var particleScale = _particlesScale * (maybeParticleSize ?? 1f);
                var totalDuration = MoveTime / moveTimeSpeedupFactor;

                // --- Phase 1: The "Pop" Area ---
                // Randomize a small circle around the start point
                Vector2 popOffset = UnityEngine.Random.insideUnitCircle * (100f * appearDistanceScale);
                Vector2 poppedPos = startingPos + popOffset;

                // --- Phase 2: The "Swerve" Path ---
                // Create a unique arc for each particle
                Vector2 tangent = new Vector2(-(targetPos.y - startingPos.y), targetPos.x - startingPos.x).normalized;
                float swerveAmount = UnityEngine.Random.Range(-200f, 200f);
                Vector2 midPoint = Vector2.Lerp(poppedPos, targetPos, 0.5f) + (tangent * swerveAmount);

                Vector3[] flyingPath = new Vector3[] { poppedPos, midPoint, targetPos };

                // --- The Sequence ---
                var seq = DOTween.Sequence().KillWith(this);

                // STEP 1: POP OUT (The explosion)
                // Individualized stagger makes it look like a shower of stars
                float individualDelay = index * 0.02f * delaySpeedupFactor;
                seq.AppendInterval(individualDelay);

                seq.Append(itemTransform.DOMove(poppedPos, 0.3f).SetEase(Ease.OutBack))
                    .Join(itemTransform.DOScale(particleScale, 0.3f).SetEase(Ease.OutBack));

                // STEP 2: BRIEF HOVER (Optional "weightless" feel)
                seq.AppendInterval(UnityEngine.Random.Range(0f, 0.1f));

                // STEP 3: THE SUCK (Accelerating to target)
                seq.Append(itemTransform.DOPath(flyingPath, totalDuration * 0.8f, PathType.CatmullRom)
                        .SetEase(Ease.InBack))
                    .Join(itemTransform.DOScale(particleScale * 0.6f, totalDuration * 0.8f).SetEase(Ease.InCubic));

                // STEP 4: IMPACT
                seq.AppendCallback(() =>
                {
                    PlayImpactFeedback(endWidget, endWidgetParent, targetContainer);
                    callbackForEachAnimation?.Invoke();
                    item.TrySetActive(false);
                });

                seq.KillWith(this);
                index++;
                return seq.AsPromiseWithKillOnCancel(true);
            });

            return new ValueTuple<IPromise, IPromise>(Promise.All(promises), Promise.Race(promises));
        }

        private void PlayImpactFeedback(Transform widget, Transform parent, RectTransform container)
        {
            // High-energy "Punch" effect for the whole UI group
            if (widget != null)
            {
                widget.DOKill(true);
                widget.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.2f, 15, 0.5f).KillWith(this);
            }

            if (parent != null)
            {
                parent.DOKill(true);
                parent.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.25f, 10, 0.5f).KillWith(this);
            }

            if (container != null)
            {
                container.DOKill(true);
                // Container "jiggles" slightly to show weight of the reward
                container.DOShakeAnchorPos(0.2f, 15f, 30, 90, false, true);
                container.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 0.5f).KillWith(this);
            }
        }

        private void Awake()
        {
            CreateCoinsPool(_itemPrefab.gameObject);
        }
    }
}