using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class ParticleBurstEmissionScaler : MonoBehaviour, IScalableWidget
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private int baseScreenWidth = 1388;
        
        private List<ParticleSystem.Burst[]> initialBursts = new List<ParticleSystem.Burst[]>();
        private int lastWidth;

        public void UpdateWidget(bool byForce = false)
        {
            TryUpdateEmissionData();
        }

        public void AnimateWidget(bool enable)
        {
            
        }
        
        private void Awake()
        {
            SaveParticlesEmissionData();
        }
        
        private void TryUpdateEmissionData()
        {
            CheckScreenWidthAndUpdateParticles();
        }

        private void SaveParticlesEmissionData()
        {
            if (particleSystems == null || particleSystems.Length == 0)
                return;

            foreach (var ps in particleSystems)
            {
                if (ps == null)
                    continue;
                
                if (ps.emission.burstCount <= 0)
                {
                    initialBursts.Add(null);
                    continue;
                }
                var emission = ps.emission;
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
                emission.GetBursts(bursts);
                initialBursts.Add(bursts);
            }
        }

        private void CheckScreenWidthAndUpdateParticles()
        {
            if (UnityEngine.Device.Screen.width != lastWidth)
            {
                UpdateParticlesEmissionData();
                lastWidth = UnityEngine.Device.Screen.width;
            }
        }

        private void UpdateParticlesEmissionData()
        {
            if (particleSystems.IsNullOrEmpty() || initialBursts.IsNullOrEmpty() || initialBursts.Count != particleSystems.Length) 
                return;

            var coefficient = (float)UnityEngine.Device.Screen.width / baseScreenWidth;

            for (var i = 0; i < particleSystems.Length; i++)
            {
                var ps = particleSystems[i];
                if (ps == null)
                    continue;
                
                var emission = ps.emission;
            
                var currentBursts = new ParticleSystem.Burst[initialBursts[i].Length];
                System.Array.Copy(initialBursts[i], currentBursts, initialBursts[i].Length);
            
                for (var j = 0; j < currentBursts.Length; j++)
                {
                    var mode = initialBursts[i][j].count.mode;

                    if (mode == ParticleSystemCurveMode.Constant)
                    {
                        currentBursts[j].count = initialBursts[i][j].count.constant * coefficient;
                    }
                    else if (mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        currentBursts[j].count = new ParticleSystem.MinMaxCurve(
                            initialBursts[i][j].count.constantMin * coefficient,
                            initialBursts[i][j].count.constantMax * coefficient
                        );
                    }
                }
            
                emission.SetBursts(currentBursts);
            }
        }
    }
}