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
            if (_uiParticle == null) return;
            
            var rt = GetComponent<RectTransform>();
            if (rt == null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
    
            _uiParticle.scale = _baseScale * canvas.scaleFactor;
        }
    }
}