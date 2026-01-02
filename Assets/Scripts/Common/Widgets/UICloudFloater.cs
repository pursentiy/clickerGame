using UnityEngine;
using DG.Tweening;

namespace Common.Widgets
{
    public class UICloudFloater : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform cloudRect;

        [Header("Positioning")]
        [Range(0f, 1f)]
        [SerializeField] private float heightCoefficient = 0.1f;

        [Header("Randomization Ranges")]
        [SerializeField] private Vector2 durationRange = new Vector2(10f, 20f);
        [SerializeField] private Vector2 delayRange = new Vector2(0f, 5f);

        private float _leftEdge;
        private float _rightEdge;
        private float _targetY;

        private void Start()
        {
            if (cloudRect == null)
            {
                cloudRect = GetComponent<RectTransform>();
            }

            // 1. Calculate Canvas bounds once
            var parentCanvas = GetComponentInParent<Canvas>();
            var canvasRect = parentCanvas.GetComponent<RectTransform>();
            
            var screenWidth = canvasRect.rect.width;
            var screenHeight = canvasRect.rect.height;

            // Define the boundaries
            var cloudHalfWidth = cloudRect.rect.width / 2f;
            _leftEdge = (-screenWidth / 2f) - cloudHalfWidth;
            _rightEdge = (screenWidth / 2f) + cloudHalfWidth;

            // 2. Set Y position once based on coefficient
            //_targetY = (-screenHeight / 2f) - (screenHeight * heightCoefficient);
            //_targetY = screenHeight - (screenHeight * heightCoefficient);
            _targetY = - (screenHeight * (1 - heightCoefficient));

            // 3. Start the recursive cycle
            StartCloudCycle(true);
        }

        private void StartCloudCycle(bool isFirstRun)
        {
            // Randomize Direction: 0 = Left to Right, 1 = Right to Left
            var isMovingRight = Random.value > 0.5f;

            var startX = isMovingRight ? _leftEdge : _rightEdge;
            var endX = isMovingRight ? _rightEdge : _leftEdge;

            // Set position to the chosen start side
            cloudRect.anchoredPosition = new Vector2(startX, _targetY);

            // Randomize parameters for this specific pass
            var randomDuration = Random.Range(durationRange.x, durationRange.y);
            var randomDelay = Random.Range(delayRange.x, delayRange.y);

            // Animate
            cloudRect.DOAnchorPosX(endX, randomDuration)
                .SetDelay(randomDelay)
                .SetEase(Ease.Linear)
                .OnComplete(() => StartCloudCycle(false));
            // .KillWith(this); // Uncomment if using your custom extension
        }

        private void OnDestroy()
        {
            cloudRect.DOKill();
        }
    }
}