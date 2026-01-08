using Common.Utilities;
using Extensions;
using RSG;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Rewards
{
    public abstract class RewardAnimationBase : MonoBehaviour
    {
        protected const float DelayAfterAdditionalAnimation = 0.25f;
        protected const float FinalStageDelayCoef = 0.033f;
        protected const float MoveTime = 1f;
        
        [Range(0.1f, 1f)]
        [SerializeField] protected DecimalRange.FloatRange _appearDistance = new DecimalRange.FloatRange(0.1f, 0.2f);
        [SerializeField] protected float _midPointCoeff = 8;
        [SerializeField] protected AnimationCurve _scaleCurve;
        [Range(0.1f, 0.9f)]
        [SerializeField] protected float _stagesCoef = 0.26f;
        [SerializeField] protected AnimationCurve _moveInCurve;
        [SerializeField] protected Canvas _canvas;
        
        protected GameObject[] ItemsPool;
        protected Transform ThisRT;
        protected readonly float DelayCoef = 0.033f;
        
        public abstract int PoolCount { get; }
        
        protected abstract (IPromise AllParticlesFinish, IPromise AnyParticleFinish) PerformAnimation(
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
        );

        public (IPromise AllParticlesFinish, IPromise AnyParticleFinish) AnimateAndWaitFor(Vector2 startingPos, Vector2 targetPos, float? maybeParticleSize, long count, float rewardsMoveTimeSpeedupFactor = 1f, RectTransform targetContainer = null, Transform endWidget = null, Transform endWidgetParent = null, float particleSpacing = 1f, UnityAction callbackForEachAnimation = null, float delaySpeedupFactor = 1f)
        {
            return PerformAnimation(startingPos, targetPos, maybeParticleSize, count, rewardsMoveTimeSpeedupFactor, targetContainer, endWidget, endWidgetParent, particleSpacing, callbackForEachAnimation, delaySpeedupFactor);
        }

        public IPromise AnimateAndWaitAll(Vector2 startingPos, Vector2 targetPos, float? maybeParticleSize, long count, float rewardsMoveTimeSpeedupFactor = 1f, RectTransform targetContainer = null, Transform endWidget = null, Transform endWidgetParent = null, float particleSpacing = 1f, float delaySpeedupFactor = 1f)
        {
            return PerformAnimation(startingPos, targetPos, maybeParticleSize, count, rewardsMoveTimeSpeedupFactor, targetContainer, endWidget, endWidgetParent, particleSpacing, delaySpeedupFactor: delaySpeedupFactor).AllParticlesFinish;
        }

        public IPromise AnimateAndWaitOnlyFirst(Vector2 startingPos, Vector2 targetPos, float? maybeParticleSize, long count, float rewardsMoveTimeSpeedupFactor = 1f, RectTransform targetContainer = null, Transform endWidget = null, Transform endWidgetParent = null, float particleSpacing = 1f, float delaySpeedupFactor = 1f)
        {
            return PerformAnimation(startingPos, targetPos, maybeParticleSize, count, rewardsMoveTimeSpeedupFactor, targetContainer, endWidget, endWidgetParent, particleSpacing, delaySpeedupFactor: delaySpeedupFactor).AnyParticleFinish;
        }

        protected void CreateCoinsPool(GameObject prefab)
        {
            if (ItemsPool?.Length > 0)
            {
                ItemsPool.Foreach(rt => Destroy(rt.gameObject));
            }

            ItemsPool = new GameObject[PoolCount];
            PoolCount.Times(i =>
            {
                var rt = Instantiate(prefab.gameObject, transform);
                rt.TrySetActive(false);
                ItemsPool[i] = rt;
            });
        }
    }
}