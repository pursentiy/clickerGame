using System.Collections.Generic;
using System.Linq;
using Common.Widgets.ContainerScaler;
using Extensions;
using Handlers.UISystem;
using Services;
using Services.Base;
using UnityEngine;
using Zenject;

namespace Handlers
{
    //TODO SCALING LOGIC
    public class ScreenTransitionParticlesHandler : DisposableMonoBehaviourService, IScalableWidget
    {
        private const string CanvasName = "[ParticlesCanvas]";
        
        [Inject] private readonly UISystemData _settings;
        
        [SerializeField] private ParticleStretchWidget _particleStretchWidget;
        [SerializeField] private ParticleBurstEmissionScalerWidget _particleBurstEmissionScalerWidget;
        
        private Canvas _uiParticlesCanvas;
        private List<ParticleSystem> _changeScreenParticles;
        
        public void PlayParticles()
        {
            if (_changeScreenParticles.IsNullOrEmpty())
                return;
            
            foreach (var particles in _changeScreenParticles)
            {
                if (particles == null)
                    continue;
                
                if (particles.isPlaying)
                {
                    particles.Stop();
                }

                particles.Simulate(0);
                particles.Play();
            }
        }

        public void UpdateWidget(bool byForce = false)
        {
            _particleStretchWidget.UpdateWidget(byForce);
            _particleBurstEmissionScalerWidget.UpdateWidget(byForce);
        }

        public void AnimateWidget(bool enable)
        {
            _particleStretchWidget.AnimateWidget(enable);
            _particleBurstEmissionScalerWidget.AnimateWidget(enable);
        }
        
        protected override void OnInitialize()
        {
            InitializeParticles();
        }

        protected override void OnDisposing()
        {
            
        }

        private void InitializeParticles()
        {
            if (_uiParticlesCanvas != null)
            {
                LoggerService.LogWarning(this, "Attempt to inititalize already initialized handler");
                return;
            }
            
            _uiParticlesCanvas = SpawnCanvas(_settings.UICanvasPrefab, CanvasName);
            
            var particles = Instantiate(_settings.UiScreenParticlesContainerPrefab, _uiParticlesCanvas.transform).GetComponentsInChildren<ParticleSystem>();
            if (particles.IsNullOrEmpty())
            {
                LoggerService.LogWarning(this, $"{nameof(ParticleSystem)} array cannot be null or empty");
                return;
            }

            _changeScreenParticles = particles.ToList();
            
            _particleStretchWidget.InitializeWidget(_changeScreenParticles);
            _particleBurstEmissionScalerWidget.InitializeWidget(_changeScreenParticles);
            UpdateWidget(true);
        }

        private Canvas SpawnCanvas(GameObject prefab, string canvasName)
        {
            var canvas = Instantiate(prefab).GetComponent<Canvas>();
            canvas.gameObject.name = canvasName;
            canvas.GetComponent<RectTransform>().SetFullStretch();

            return canvas;
        }
    }
}