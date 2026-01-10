using UnityEngine;

namespace Common.Widgets
{
    public class WorldShadowController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _shadowSR;

        [Header("Shadow Settings")] [SerializeField]
        private float _baseScaleY = 0.5f;

        [SerializeField] private float _maxShadowLength = 3f;
        [SerializeField] private float _minShadowLength = 0.5f;
        [SerializeField] private float _alphaIntensity = 0.4f;

        [Header("Fade Bounds (Angles)")]
        [Tooltip("The angle at which the shadow is fully visible (usually near noon/top of arc)")]
        [SerializeField]
        private float _peakAngle = 90f;

        [Tooltip("Shadow fades to 0 as it reaches these angles (e.g., 0 and 180 for horizon)")] [SerializeField]
        private float _minAngle = 0f;

        [SerializeField] private float _maxAngle = 180f;

        [Tooltip("How sharp the fade is as it approaches the limits")] [SerializeField]
        private float _fadeSmoothing = 10f;

        public void UpdateShadows(Vector3 sunPosition)
        {
            // 1. Вектор от солнца к объекту
            var direction = transform.position - sunPosition;

            // 2. Логика вращения
            var angleRad = Mathf.Atan2(direction.y, direction.x);
            var angleDeg = angleRad * Mathf.Rad2Deg;
            var normalizedAngle = (angleDeg + 360f) % 360f;

            transform.rotation = Quaternion.Euler(0, 0, angleDeg + 90f);

            // 3. Логика масштабирования
            var shadowLength = _maxShadowLength - (direction.y * 0.5f);
            shadowLength = Mathf.Clamp(shadowLength, _minShadowLength, _maxShadowLength);
            transform.localScale = new Vector3(_baseScaleY, shadowLength, 1f);

            // 4. Расширенная логика затухания (Углы + Горизонт)
            var color = _shadowSR.color;
            var finalAlpha = 0f;

            if (normalizedAngle > _minAngle && normalizedAngle < _maxAngle)
            {
                // А) Затухание по углу (как раньше)
                float angleFade = (normalizedAngle <= _peakAngle)
                    ? Mathf.InverseLerp(_minAngle, _peakAngle, normalizedAngle)
                    : Mathf.InverseLerp(_maxAngle, _peakAngle, normalizedAngle);

                // Б) Затухание за горами (по высоте Y)
                // Допустим, на высоте Y = 0 солнце скрывается за горами. 
                // Мы плавно гасим тень, когда солнце приближается к этой отметке.
                float horizonY = 0f; // Настройте под уровень ваших гор в мире
                float fadeRangeY = 1.5f; // Дистанция плавного исчезновения

                float heightFade = Mathf.InverseLerp(horizonY, horizonY + fadeRangeY, sunPosition.y);

                // Итоговая прозрачность — это пересечение двух условий
                finalAlpha = angleFade * heightFade;
                finalAlpha = Mathf.SmoothStep(0, 1, finalAlpha);
            }

            color.a = finalAlpha * _alphaIntensity;
            _shadowSR.color = color;
        }
    }
}