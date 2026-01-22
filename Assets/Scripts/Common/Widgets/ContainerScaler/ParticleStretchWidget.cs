using Extensions;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class ParticleStretchWidget : MonoBehaviour, IScalableWidget
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [Range(0.1f, 2f)]
        [SerializeField] private float offsetMultiplier = 0f;
        
        private ParticleSystem.EmissionModule emission;
        private int lastWidth;

        public void UpdateWidget(bool byForce = false)
        {
            CheckScreenWidthAndUpdateParticles();
        }

        public void AnimateWidget(bool enable)
        {
            
        }

        private void CheckScreenWidthAndUpdateParticles()
        {
            if (UnityEngine.Device.Screen.width != lastWidth)
            {
                StretchParticles();
                lastWidth = UnityEngine.Device.Screen.width;
            }
        }
        
        private void StretchParticles()
        {
            if (Camera.main == null || particleSystems.IsNullOrEmpty()) 
                return;

            foreach (var system in particleSystems)
            {
                var shape = system.shape;
                shape.scale = new Vector3(UnityEngine.Device.Screen.width * offsetMultiplier, 0, 0);
            }
        }
    }
}