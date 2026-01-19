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
        private bool _canUpdate = true;

        private void Awake()
        {
            _lastOrientation = UnityEngine.Device.Screen.orientation;
            _lastWidth = UnityEngine.Device.Screen.width;
            _lastHeight = UnityEngine.Device.Screen.height;
        }
        
        private void Update()
        {
            if (HasScreenChanged() && _canUpdate)
            {
                _lastOrientation = UnityEngine.Device.Screen.orientation;
                _lastWidth = UnityEngine.Device.Screen.width;
                _lastHeight = UnityEngine.Device.Screen.height;
                
                TryUpdateWidgets();
            }
        }
        
        private void OnDisable() 
        {
            _canUpdate = false;
            TryStopAllAnimations();
        }

        private void OnEnable()
        {
            _canUpdate = true;
            
            TryUpdateWidgets();
            StarCloudsAnimation(true);
        }

        private bool HasScreenChanged()
        {
            return UnityEngine.Device.Screen.orientation != _lastOrientation ||
                   UnityEngine.Device.Screen.width != _lastWidth ||
                   UnityEngine.Device.Screen.height != _lastHeight;
        }

        private void TryUpdateWidgets()
        {
            _particleStretchWidget.TryUpdateParticlesStretch();
            _particleBurstEmissionScaler.TryUpdateEmissionData();
            _backgroundFitter.ApplyUniversalFill();
            
            LoggerService.LogDebugEditor($"[{GetType().Name}] Widgets updated for resolution: {_lastWidth}x{_lastHeight}");
        }

        private void TryStopAllAnimations()
        {
            StarCloudsAnimation(false);
        }
        
        private void StarCloudsAnimation(bool enable)
        {
            if (_cloudFloaters.IsNullOrEmpty())
                return;

            foreach (var cloudFloater in _cloudFloaters)
            {
                if (cloudFloater == null) 
                    continue;
                
                if (enable)
                    cloudFloater.StartAnimation();
                else
                    cloudFloater.StopAnimation();
            }
        }
    }
}