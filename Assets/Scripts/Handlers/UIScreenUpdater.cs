using Common.Widgets;
using UnityEngine;

namespace Handlers
{
    public class UIScreenUpdater : MonoBehaviour
    {
        [SerializeField] private ParticleStretchWidget _particleStretchWidget;
        [SerializeField] private ParticleBurstEmissionScaler _particleBurstEmissionScaler;

        private void Start()
        {
            UpdateParticles();
        }

        private void UpdateParticles()
        {
            _particleStretchWidget.TryUpdateParticlesStretch();
            _particleBurstEmissionScaler.TryUpdateEmissionData();
        }
        
#if UNITY_EDITOR
        private void Update() => UpdateParticles();
#endif
    }
}