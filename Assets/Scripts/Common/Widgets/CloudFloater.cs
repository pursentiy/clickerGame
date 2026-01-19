using UnityEngine;
using DG.Tweening;
using Extensions;
using Utilities.Disposable;

namespace Common.Widgets
{
    public class CloudFloater: MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cloudTransform;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Positioning")]
        [Range(0f, 1f)]
        [Tooltip("0 = Bottom of screen, 1 = Top of screen")]
        [SerializeField] private float heightCoefficient = 0.8f;

        [Header("Randomization Ranges")]
        [SerializeField] private Vector2 durationRange = new Vector2(10f, 20f);
        [SerializeField] private Vector2 delayRange = new Vector2(0f, 5f);
        [SerializeField] private float _disablingChance = 0.3f;

        private float _leftEdge;
        private float _rightEdge;
        private float _targetY;

        public void StopAnimation()
        {
            cloudTransform.DOKill(false);
        }

        public void StartAnimation()
        {
            if (Random.Range(0f, 1f) < _disablingChance)
            {
                cloudTransform.TrySetActive(false);
                return;
            }

            cloudTransform.TrySetActive(true);
            
            // 1. Calculate World bounds using the Camera
            var cam = Camera.main;
            var leftWorldPoint = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            var rightWorldPoint = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
            var topWorldPoint = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
            var bottomWorldPoint = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));

            var worldWidth = rightWorldPoint.x - leftWorldPoint.x;
            var worldHeight = topWorldPoint.y - bottomWorldPoint.y;

            // Define the boundaries based on Sprite size
            var cloudWidthWithOffset = spriteRenderer != null ? spriteRenderer.bounds.extents.x * 2.15f : 1f;
            _leftEdge = leftWorldPoint.x - cloudWidthWithOffset;
            _rightEdge = rightWorldPoint.x + cloudWidthWithOffset;

            // 2. Set Y position based on world height
            // We start from the bottom edge and add a percentage of the total height
            _targetY = bottomWorldPoint.y + (worldHeight * heightCoefficient);

            // 3. Start the recursive cycle
            StartCloudCycle(true);
        }

        private void StartCloudCycle(bool isFirstRun)
        {
            var isMovingRight = Random.value > 0.5f;
            var startX = isMovingRight ? _leftEdge : _rightEdge;
            var endX = isMovingRight ? _rightEdge : _leftEdge;

            // Reset position
            cloudTransform.position = new Vector3(startX, _targetY, cloudTransform.position.z);

            var randomDuration = Random.Range(durationRange.x, durationRange.y);
            var randomDelay = isFirstRun ? 0f : Random.Range(delayRange.x, delayRange.y);

            // 1. Curated list of "Cloud-like" eases
            // Linear: Constant wind
            // InOutSine/Quad: Gentle gust that starts slow and settles
            var eases = new[] { Ease.Linear, Ease.InOutSine, Ease.InOutQuad, Ease.InSine, Ease.OutSine };
            var selectedEase = eases[Random.Range(0, eases.Length)];

            // 2. Main Horizontal Movement
            cloudTransform.DOMoveX(endX, randomDuration)
                .SetDelay(randomDelay)
                .SetEase(selectedEase)
                .OnComplete(() => StartCloudCycle(false))
                .SetLink(gameObject)
                .KillWith(this);

            // 3. Optional: Add a "Floating" vertical drift for cartoon style
            // This makes the cloud bob up and down slightly as it moves
            var driftAmount = Random.Range(0.1f, 0.3f);
            cloudTransform.DOMoveY(_targetY + driftAmount, randomDuration / 4f)
                .SetDelay(randomDelay)
                .SetEase(Ease.InOutSine)
                .SetLoops(4, LoopType.Yoyo)
                .KillWith(this);
        }
    }
}