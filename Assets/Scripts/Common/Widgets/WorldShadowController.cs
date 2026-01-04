using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.Widgets
{public class WorldShadowController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _shadowSR;

    [Header("Shadow Settings")]
    [SerializeField] private float _baseScaleY = 0.5f; 
    [SerializeField] private float _maxShadowLength = 3f;
    [SerializeField] private float _minShadowLength = 0.5f;
    [SerializeField] private float _alphaIntensity = 0.4f;

    [Header("Fade Bounds (Angles)")]
    [Tooltip("The angle at which the shadow is fully visible (usually near noon/top of arc)")]
    [SerializeField] private float _peakAngle = 90f;
    [Tooltip("Shadow fades to 0 as it reaches these angles (e.g., 0 and 180 for horizon)")]
    [SerializeField] private float _minAngle = 0f;
    [SerializeField] private float _maxAngle = 180f;
    [Tooltip("How sharp the fade is as it approaches the limits")]
    [SerializeField] private float _fadeSmoothing = 10f;

    public void UpdateShadows(Vector3 sunPosition)
    {
        // 1. Vector from Sun to Object
        var direction = transform.position - sunPosition;
    
        // 2. Rotation Logic
        var angleRad = Mathf.Atan2(direction.y, direction.x);
        var angleDeg = angleRad * Mathf.Rad2Deg;
        
        // Normalize angle to 0-360 range for easier calculation
        var normalizedAngle = Math.Abs(angleDeg + 360) % 360;

        // Point shadow away from sun
        transform.rotation = Quaternion.Euler(0, 0, angleDeg + 90f);

        // 3. Scaling Logic
        var shadowLength = _maxShadowLength - (direction.y * 0.5f);
        shadowLength = Mathf.Clamp(shadowLength, _minShadowLength, _maxShadowLength);
        transform.localScale = new Vector3(_baseScaleY, shadowLength, 1f);

        // 4. Advanced Fade Logic
        var color = _shadowSR.color;
        
        // Calculate how close we are to the horizon limits
        // Returns 1 at peakAngle and 0 at minAngle/maxAngle
        var fadeAlpha = 0f;

        if (normalizedAngle > _minAngle && normalizedAngle < _maxAngle)
        {
            // Calculate distance to nearest edge
            var distToMin = Mathf.Abs(normalizedAngle - _minAngle);
            var distToMax = Mathf.Abs(normalizedAngle - _maxAngle);
            var closestEdgeDist = Mathf.Min(distToMin, distToMax);

            // Smoothly interpolate alpha based on distance from the "kill" angles
            fadeAlpha = Mathf.Clamp01(closestEdgeDist / _fadeSmoothing);
        }

        // Combine the proximity fade with your base intensity
        color.a = fadeAlpha * _alphaIntensity;
        _shadowSR.color = color;
    }
}
}