using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class ParticleStretchWidget : MonoBehaviour, IScalableWidget
    {
        [Range(0.1f, 2f)]
        [SerializeField] private float offsetMultiplier = 0f;
        
        private ParticleSystem.EmissionModule _emission;
        private int _lastWidth;
        private List<ParticleSystem> _particleSystems;
        private bool _initialized;

        public void InitializeWidget(List<ParticleSystem> particleSystems)
        {
            _particleSystems =  particleSystems;
            _initialized = true;
        }

        public void UpdateWidget(bool byForce = false)
        {
            CheckScreenWidthAndUpdateParticles();
        }

        public void AnimateWidget(bool enable)
        {
            
        }

        private void CheckScreenWidthAndUpdateParticles()
        {
            if (!_initialized)
                return;
            
            if (UnityEngine.Device.Screen.width != _lastWidth)
            {
                StretchParticles();
                _lastWidth = UnityEngine.Device.Screen.width;
            }
        }
        
        private void StretchParticles()
        {
            if (!_initialized || Camera.main == null || _particleSystems.IsNullOrEmpty()) 
                return;

            var scale = UnityEngine.Device.Screen.width * offsetMultiplier;
            foreach (var system in _particleSystems)
            {
                if (system == null)
                    continue;
                
                var shape = system.shape;
                shape.scale = new Vector3(scale, 0, 0);
            }
        }
    }
}