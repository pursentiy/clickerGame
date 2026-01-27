using Extensions;
using Plugins.FSignal;
using Services.Base;
using UnityEngine;
using Utilities.Disposable;

namespace Services.ScreenObserver
{
    public class ScreenObserverService : DisposableService
    {
        public FSignal<ScreenOrientation> OnOrientationChangeSignal = new();
        public FSignal OnResolutionChangeSignal = new();
        
        private ScreenOrientation _lastOrientation;
        private int _lastWidth;
        private int _lastHeight;
        private Coroutine _coroutine;
        private DisplayEventProxy _proxy;

        public ScreenObserverService()
        {
            SetupDetectorObject();
            SaveInitialScreenParams();
        }

        private void SetupDetectorObject()
        {
            _proxy = DisplayEventProxy.Create();
            _proxy.ScreenChangedSignal.MapListener(CheckScreenParams).DisposeWith(this);
        }
        
        protected override void OnInitialize()
        {
        }

        protected override void OnDisposing()
        {
            if (_proxy != null)
            {
                Object.Destroy(_proxy.gameObject);
            }
        }
        
        private void SaveInitialScreenParams()
        {
            _lastOrientation = UnityEngine.Device.Screen.orientation;
            _lastWidth = UnityEngine.Device.Screen.width;
            _lastHeight = UnityEngine.Device.Screen.height;
        }
        
        private void CheckScreenParams()
        {
            if (HasScreenResolutionChanged())
            {
                _lastWidth = UnityEngine.Device.Screen.width;
                _lastHeight = UnityEngine.Device.Screen.height;
                OnResolutionChangeSignal.Dispatch();
            }

            if (HasScreenOrientationChanged())
            {
                _lastOrientation = UnityEngine.Device.Screen.orientation;
                OnOrientationChangeSignal.Dispatch(_lastOrientation);
            }
        }
        
        private bool HasScreenResolutionChanged()
        {
            return UnityEngine.Device.Screen.width != _lastWidth ||
                    UnityEngine.Device.Screen.height != _lastHeight;
        }

        private bool HasScreenOrientationChanged()
        {
            return UnityEngine.Device.Screen.orientation != _lastOrientation;
        }
    }
}