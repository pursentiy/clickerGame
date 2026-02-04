using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class ParticleBurstEmissionScalerWidget : MonoBehaviour
    {
        [SerializeField] private int baseScreenWidth = 1388;
        
        private List<ParticleSystem.Burst[]> _initialBursts = new ();
        private int _lastWidth;
        private List<ParticleSystem> _particleSystems;
        private bool _initialized;

        public void InitializeWidget(List<ParticleSystem> particleSystems)
        {
            _particleSystems =  particleSystems;
            _initialized = true;
            SaveParticlesEmissionData();
        }
        
        public void UpdateWidget(bool byForce = false)
        {
            TryUpdateEmissionData();
        }

        public void AnimateWidget(bool enable)
        {
            
        }
        
        private void TryUpdateEmissionData()
        {
            CheckScreenWidthAndUpdateParticles();
        }

        private void SaveParticlesEmissionData()
        {
            if (!_initialized || _particleSystems.IsNullOrEmpty())
                return;

            foreach (var ps in _particleSystems)
            {
                if (ps == null)
                    continue;
                
                if (ps.emission.burstCount <= 0)
                {
                    _initialBursts.Add(null);
                    continue;
                }
                var emission = ps.emission;
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
                emission.GetBursts(bursts);
                _initialBursts.Add(bursts);
            }
        }

        private void CheckScreenWidthAndUpdateParticles()
        {
            if (!_initialized)
                return;
            
            if (UnityEngine.Device.Screen.width != _lastWidth)
            {
                UpdateParticlesEmissionData();
                _lastWidth = UnityEngine.Device.Screen.width;
            }
        }

        private void UpdateParticlesEmissionData()
        {
            if (!_initialized || _particleSystems.IsNullOrEmpty() || _initialBursts.IsNullOrEmpty() || _initialBursts.Count != _particleSystems.Count) 
                return;

            var coefficient = (float)UnityEngine.Device.Screen.width / baseScreenWidth;

            for (var i = 0; i < _particleSystems.Count; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;
                
                var emission = ps.emission;
            
                var currentBursts = new ParticleSystem.Burst[_initialBursts[i].Length];
                System.Array.Copy(_initialBursts[i], currentBursts, _initialBursts[i].Length);
            
                for (var j = 0; j < currentBursts.Length; j++)
                {
                    var mode = _initialBursts[i][j].count.mode;

                    if (mode == ParticleSystemCurveMode.Constant)
                    {
                        currentBursts[j].count = Mathf.Clamp(_initialBursts[i][j].count.constant * coefficient, 1, 100);
                    }
                    else if (mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        currentBursts[j].count = new ParticleSystem.MinMaxCurve(
                            _initialBursts[i][j].count.constantMin * coefficient,
                            _initialBursts[i][j].count.constantMax * coefficient
                        );
                    }
                }
            
                emission.SetBursts(currentBursts);
            }
        }
    }
}