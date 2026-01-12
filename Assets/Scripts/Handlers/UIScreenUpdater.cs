using Common.Widgets;
using Extensions;
using Services;
using UnityEngine;

namespace Handlers
{
    public class UIScreenUpdater : MonoBehaviour
    {
        [SerializeField] private ParticleStretchWidget _particleStretchWidget;
        [SerializeField] private ParticleBurstEmissionScaler _particleBurstEmissionScaler;
        [SerializeField] private BackgroundFitter _backgroundFitter;
        [SerializeField] private CloudFloater[] _cloudFloaters;

        private ScreenOrientation _lastOrientation;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _lastOrientation = UnityEngine.Device.Screen.orientation;
            _lastWidth = UnityEngine.Device.Screen.width;
            _lastHeight = UnityEngine.Device.Screen.height;
        }
        
        private void Start()
        {
            UpdateWidgets();
            StarCloudsAnimation();
        }

        private void StarCloudsAnimation()
        {
            if (_cloudFloaters.IsNullOrEmpty())
                return;

            foreach (var cloudFloater in _cloudFloaters)
            {
                if (cloudFloater != null)
                    cloudFloater.StartAnimation();
            }
        }
        
        private void Update()
        {
            if (HasScreenChanged())
            {
                _lastOrientation = UnityEngine.Device.Screen.orientation;
                _lastWidth = UnityEngine.Device.Screen.width;
                _lastHeight = UnityEngine.Device.Screen.height;
                
                UpdateWidgets();
            }
        }

        private bool HasScreenChanged()
        {
            return UnityEngine.Device.Screen.orientation != _lastOrientation ||
                   UnityEngine.Device.Screen.width != _lastWidth ||
                   UnityEngine.Device.Screen.height != _lastHeight;
        }

        private void UpdateWidgets()
        {
            _particleStretchWidget.TryUpdateParticlesStretch();
            _particleBurstEmissionScaler.TryUpdateEmissionData();
            _backgroundFitter.ApplyUniversalFill();
            
            LoggerService.LogDebugEditor($"[{GetType().Name}] Widgets updated for resolution: {_lastWidth}x{_lastHeight}");
        }
        
    }
}