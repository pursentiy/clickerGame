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

        [Header("Movement Settings")] [SerializeField]
        private Vector2 arcHeightRange = new Vector2(4f, 7f); // Lower units for World Space

        [SerializeField] private Vector2 cycleDurationRange = new Vector2(12f, 20f);

        [Range(0, 1)] [SerializeField] private float widthCoefficientOffset = 0.1f;
        [Range(0.1f, 100)] [SerializeField] private float startingYCoefficient = 4f;

        [Header("Rotation Settings")] [SerializeField]
        private Vector2 sunRotationDurationRange = new Vector2(8f, 12f);

        [SerializeField] private Vector2 rayRotationDurationRange = new Vector2(8f, 12f);

        private void Start()
        {
            if (sunTransform == null) sunTransform = transform;

            // 1. Calculate World Space Bounds via Camera Viewport
            Camera cam = Camera.main;
            // Viewport (0,0.5) is left-center, (1,0.5) is right-center
            Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
            Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));
            Vector3 topEdge = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0));
            Vector3 bottomEdge = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0));

            float worldWidth = rightEdge.x - leftEdge.x;
            float worldHeight = topEdge.y - bottomEdge.y;

            float widthOffset = worldWidth * widthCoefficientOffset;

            // 2. Set Starting Points
            float startX = leftEdge.x - widthOffset;
            float endX = rightEdge.x + widthOffset;

            // startingYCoefficient determines how far below the center the sun starts
            // Bottom is center.y - (height/2), we offset from there
            float startY = cam.transform.position.y - (worldHeight / startingYCoefficient);

            sunTransform.position = new Vector3(startX, startY, sunTransform.position.z);

            float cycleDuration = Random.Range(cycleDurationRange.x, cycleDurationRange.y);
            float arcHeight = Random.Range(arcHeightRange.x, arcHeightRange.y);

            // 3. Horizontal Movement (World X)
            sunTransform.DOMoveX(endX, cycleDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);

            // 4. Arc Movement (World Y)
            sunTransform.DOMoveY(startY + arcHeight, cycleDuration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            // 5. Constant Rotations
            AnimateRotations();
        }

        private void AnimateRotations()
        {
            float sunRotationDuration = Random.Range(sunRotationDurationRange.x, sunRotationDurationRange.y);
            float rayRotationDuration = Random.Range(rayRotationDurationRange.x, rayRotationDurationRange.y);

            sunTransform.DORotate(new Vector3(0, 0, 360), sunRotationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .KillWith(this);;

            if (maybeRayTransform != null)
            {
                maybeRayTransform.DORotate(new Vector3(0, 0, -360), rayRotationDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .KillWith(this);
            }
        }
    }
}