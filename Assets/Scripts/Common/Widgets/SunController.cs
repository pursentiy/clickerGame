using DG.Tweening;
using UnityEngine;

namespace Common.Widgets
{
    public class SunController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Transform maybeRayTransform;

        [Header("Movement Settings")]
        [Tooltip("Точка пика солнца относительно центра экрана по Y")]
        [SerializeField] private float peakYOffset = 5f;
        
        [Tooltip("На сколько далеко солнце уходит за края экрана по X")]
        [SerializeField] private float horizontalMargin = 2f;

        [SerializeField] private Vector2 cycleDurationRange = new Vector2(12f, 20f);

        [Header("Rotation Settings")] 
        [SerializeField] private Vector2 sunRotationDurationRange = new Vector2(8f, 12f);
        [SerializeField] private Vector2 rayRotationDurationRange = new Vector2(8f, 12f);
        
        public Vector3 SunPosition => sunTransform.position;

        private void Start()
        {
            if (sunTransform == null) sunTransform = transform;

            // 1. Рассчитываем ключевые точки относительно камеры
            Camera cam = Camera.main;
            Vector3 centerWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
            Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));

            float startX = leftEdge.x - horizontalMargin;
            float endX = rightEdge.x + horizontalMargin;
            float centerX = centerWorld.x;
            
            // Самая высокая точка (Пик)
            float peakY = centerWorld.y + peakYOffset;
            
            // Точка старта и финиша по Y (ниже центра)
            float horizonY = centerWorld.y - (peakYOffset * 0.5f); 

            // Устанавливаем в начальную позицию
            sunTransform.position = new Vector3(startX, horizonY, sunTransform.position.z);

            float duration = Random.Range(cycleDurationRange.x, cycleDurationRange.y);

            // 2. Анимация по X (от края до края)
            sunTransform.DOMoveX(endX, duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            // 3. Анимация по Y (Вверх-Вниз)
            // Использование Ease.OutQuad/InQuad создаст более естественную параболу
            Sequence ySequence = DOTween.Sequence();
            ySequence.Append(sunTransform.DOMoveY(peakY, duration / 2).SetEase(Ease.OutQuad))
                     .Append(sunTransform.DOMoveY(horizonY, duration / 2).SetEase(Ease.InQuad));
            
            ySequence.SetLoops(-1, LoopType.Yoyo)
                     .SetLink(gameObject);

            AnimateRotations();
        }

        private void AnimateRotations()
        {
            float sunRotDur = Random.Range(sunRotationDurationRange.x, sunRotationDurationRange.y);
            float rayRotDur = Random.Range(rayRotationDurationRange.x, rayRotationDurationRange.y);

            sunTransform.DORotate(new Vector3(0, 0, 360), sunRotDur, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);

            if (maybeRayTransform != null)
            {
                maybeRayTransform.DORotate(new Vector3(0, 0, -360), rayRotDur, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .SetLink(gameObject);
            }
        }
    }
}