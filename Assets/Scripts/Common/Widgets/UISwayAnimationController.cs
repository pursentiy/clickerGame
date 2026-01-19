using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace Common.Widgets
{
    public class UISwayAnimationController : MonoBehaviour
    {
        [Header("UI Targets")]
        [SerializeField] private RectTransform bodyRect; // Используем RectTransform для UI
        [SerializeField] private RectTransform headRect;

        [Header("Timing (Randomized)")]
        [SerializeField] private Vector2 durationRange = new Vector2(1.5f, 2.5f);
        [SerializeField] private Vector2 pauseRange = new Vector2(0f, 0.2f);
        [SerializeField] private Ease swayEase = Ease.InOutSine;

        [Header("Angles (Z-axis rotation)")]
        [SerializeField] private float bodyBaseAngle = 5f;
        [SerializeField] private float headBaseAngle = -10f; 
        [SerializeField] private Vector2 angleMultiplier = new Vector2(0.8f, 1.2f);

        private void Start()
        {
            // Если не назначено, пытаемся взять компонент с текущего объекта
            if (bodyRect == null) bodyRect = GetComponent<RectTransform>();
        
            Sway(true);
        }

        void Sway(bool toRight)
        {
            var randomDuration = Random.Range(durationRange.x, durationRange.y);
            var randomPause = Random.Range(pauseRange.x, pauseRange.y);
            var randomAngleMod = Random.Range(angleMultiplier.x, angleMultiplier.y);

            var targetBodyAngle = (toRight ? bodyBaseAngle : -bodyBaseAngle) * randomAngleMod;
            var targetHeadAngle = (toRight ? headBaseAngle : -headBaseAngle) * randomAngleMod;

            // 1. Анимация Тела (UI вращается вокруг оси Z)
            if (bodyRect != null)
            {
                bodyRect.DOLocalRotate(new Vector3(0, 0, targetBodyAngle), randomDuration)
                    .SetEase(swayEase)
                    .SetLink(gameObject)
                    .OnComplete(() => {
                        DOVirtual.DelayedCall(randomPause, () => Sway(!toRight))
                            .SetLink(gameObject);
                    }).KillWith(this);
            }

            // 2. Анимация Головы
            if (headRect != null)
            {
                headRect.DOLocalRotate(new Vector3(0, 0, targetHeadAngle), randomDuration)
                    .SetEase(swayEase)
                    .SetLink(gameObject)
                    .KillWith(this);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
            
            if (bodyRect) 
                bodyRect.DOKill();
            
            if (headRect)
                headRect.DOKill();
        }
    }
}