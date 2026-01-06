using Extensions;
using UnityEngine;

namespace Common.Widgets
{
    public class ParticleStretchWidget : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [Range(0.1f, 2f)]
        [SerializeField] private float offsetMultiplier = 0f;
        
        private ParticleSystem.EmissionModule emission;
        private int lastWidth;

        public void TryUpdateParticlesStretch()
        {
            CheckScreenWidthAndUpdateParticles();
        }
        
        private void Awake()
        {
            lastWidth = UnityEngine.Device.Screen.width;
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