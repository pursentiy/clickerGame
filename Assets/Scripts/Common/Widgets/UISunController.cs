using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace Common.Widgets
{
    public class UISunController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform sunRect;    // The main Sun UI element
        [SerializeField] private RectTransform maybeRayRect;    // The nested Rays UI element

        [Header("Movement Settings")]
        [SerializeField] private Vector2 arcHeightRange = new Vector2(400f, 700f); 
        [SerializeField] private Vector2 cycleDurationRange = new Vector2(12f, 20f);
        [Range(0, 1)]
        [SerializeField] private float widthCoefficientOffset = 0.1f;
        [Range(0.1f, 100)]
        [SerializeField] private float startingYCoefficient = 4f;

        [Header("Rotation Settings")]
        [SerializeField] private Vector2 sunRotationDurationRange = new Vector2(8f, 12f);
        [SerializeField] private Vector2 rayRotationDurationRange =new Vector2(8f, 12f);

        private void Start()
        {
            if (sunRect == null)
            {
                sunRect = GetComponent<RectTransform>();
            }

            // 1. Calculate Screen/Canvas Width
            // We find the parent canvas to get the local UI coordinate bounds
            var parentCanvas = GetComponentInParent<Canvas>();
            var canvasRect = parentCanvas.GetComponent<RectTransform>();
        
            var width = canvasRect.rect.width;
            var widthOffset = width *  widthCoefficientOffset;
            var height = canvasRect.rect.height;

            // 2. Set Starting Point (Far Left, Middle Height)
            // We subtract half the width to center it if the anchor is middle
            var startX = (-width / 2f) - widthOffset;
            var endX = (width / 2f) + widthOffset;
            var startY = -height / startingYCoefficient; // Starting slightly below center

            sunRect.anchoredPosition = new Vector2(startX, startY);
            
            var cycleDuration = Random.Range(cycleDurationRange.x, cycleDurationRange.y);
            var arcHeight = Random.Range(arcHeightRange.x, arcHeightRange.y);

            // 3. Horizontal Movement (Full Screen Width)
            sunRect.DOAnchorPosX(endX, cycleDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .KillWith(this);

            // 4. Arc Movement (Rising/Setting effect)
            sunRect.DOAnchorPosY(startY + arcHeight, cycleDuration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .KillWith(this);

            // 5. Constant Rotations
            AnimateRotations();
        }

        private void AnimateRotations()
        {
            var sunRotationDuration = Random.Range(sunRotationDurationRange.x, sunRotationDurationRange.y);
            var rayRotationDuration = Random.Range(rayRotationDurationRange.x, rayRotationDurationRange.y);
            
            // Sun Core
            sunRect.DORotate(new Vector3(0, 0, 360), sunRotationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .KillWith(this);

            // Nested Rays (Counter-rotation for visual depth)
            if (maybeRayRect != null)
            {
                maybeRayRect.DORotate(new Vector3(0, 0, -360), rayRotationDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .KillWith(this);
            }
        }
    }
}