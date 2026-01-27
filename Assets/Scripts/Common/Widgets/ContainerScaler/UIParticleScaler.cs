using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

// Если используешь UI Particle от Coffee

namespace Common.Widgets.ContainerScaler
{
    //TODO FIX PARTICLES SCALING
    public class UIParticleScaler : MonoBehaviour
    {
        [SerializeField] private UIParticle _uiParticle;
        [SerializeField] private float _baseScale = 1f;
    
        private CanvasScaler _scaler;

        void Awake()
        {
            _scaler = GetComponentInParent<CanvasScaler>();
            UpdateScale();
        }
        
        private void UpdateScale()
        {
            if (_scaler == null || _uiParticle == null)
                return;

            // Рассчитываем коэффициент масштабирования Canvas
            var screenScale = Screen.width / _scaler.referenceResolution.x;
            _uiParticle.scale = _baseScale * screenScale;
        }
    }
}