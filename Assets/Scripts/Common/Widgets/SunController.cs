using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace Common.Widgets
{
    public class SunController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Transform maybeRayTransform;

        [Header("Movement Settings")]
        [Tooltip("Точка пика солнца относительно центра экрана по Y")]
        [SerializeField] private Vector2 leftPosition;
        [SerializeField] private Vector2 peakPosition;
        [SerializeField] private Vector2 rightPosition;

        [SerializeField] private Vector2 cycleDurationRange = new Vector2(12f, 20f);

        [Header("Rotation Settings")] 
        [SerializeField] private Vector2 sunRotationDurationRange = new Vector2(8f, 12f);
        [SerializeField] private Vector2 rayRotationDurationRange = new Vector2(8f, 12f);
        
        public Vector3 SunPosition => sunTransform.position;

        private void Start()
        {
            StarSunAnimation();
        }

        private void StarSunAnimation()
        {
            if (sunTransform == null) 
                sunTransform = transform;
            
            sunTransform.localPosition = new Vector3(leftPosition.x, leftPosition.y, sunTransform.position.z);

            var duration = Random.Range(cycleDurationRange.x, cycleDurationRange.y);
            
            sunTransform.DOLocalMoveX(rightPosition.x, duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject)
                .KillWith(this);

            var ySequence = DOTween.Sequence().KillWith(this);
            ySequence.Append(sunTransform.DOLocalMoveY(peakPosition.y, duration / 2).SetEase(Ease.OutQuad))
                .Append(sunTransform.DOLocalMoveY(rightPosition.y, duration / 2).SetEase(Ease.InQuad));
            
            ySequence.SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            AnimateRotations();
        }

        private void AnimateRotations()
        {
            var sunRotDur = Random.Range(sunRotationDurationRange.x, sunRotationDurationRange.y);
            var rayRotDur = Random.Range(rayRotationDurationRange.x, rayRotationDurationRange.y);

            sunTransform.DORotate(new Vector3(0, 0, 360), sunRotDur, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject)
                .KillWith(this);

            if (maybeRayTransform != null)
            {
                maybeRayTransform.DORotate(new Vector3(0, 0, -360), rayRotDur, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .SetLink(gameObject)
                    .KillWith(this);
            }
        }
    }
}