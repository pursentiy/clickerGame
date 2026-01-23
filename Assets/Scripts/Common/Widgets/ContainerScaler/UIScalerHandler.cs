using System.Collections.Generic;
using Extensions;
using Services;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class UIScalerHandler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _scalableWidgetsGameObjects;
        
        private List<IScalableWidget> _scalableWidgets = new ();
        private ScreenOrientation _lastOrientation;
        private int _lastWidth;
        private int _lastHeight;
        private bool _canUpdate = true;

        private void Awake()
        {
            GetScalableWidgets();
            
            _lastOrientation = UnityEngine.Device.Screen.orientation;
            _lastWidth = UnityEngine.Device.Screen.width;
            _lastHeight = UnityEngine.Device.Screen.height;
        }

        private void GetScalableWidgets()
        {
            if (_scalableWidgetsGameObjects.IsCollectionNullOrEmpty())
                return;

            foreach (var scalableWidgetsGameObject in _scalableWidgetsGameObjects)
            {
                if (scalableWidgetsGameObject == null)
                    continue;

                var widgets = scalableWidgetsGameObject.GetComponents<IScalableWidget>();
                
                if (widgets.IsCollectionNullOrEmpty()) 
                    continue;
                
                foreach (var scalableWidget in widgets)
                {
                    if (scalableWidget != null)
                        _scalableWidgets.Add(scalableWidget);
                }
            }
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
            TryAnimateWidget(false);
        }

        private void OnEnable()
        {
            _canUpdate = true;
            
            TryUpdateWidgets();
            TryAnimateWidget(true);
        }

        private bool HasScreenChanged()
        {
            return UnityEngine.Device.Screen.orientation != _lastOrientation ||
                   UnityEngine.Device.Screen.width != _lastWidth ||
                   UnityEngine.Device.Screen.height != _lastHeight;
        }

        private void TryUpdateWidgets()
        {
            if (_scalableWidgets.IsCollectionNullOrEmpty())
                return;
            
            foreach (var scalableWidget in _scalableWidgets)
            {
                scalableWidget?.UpdateWidget();
            }
        }

        private void TryAnimateWidget(bool enable)
        {
            if (_scalableWidgets.IsCollectionNullOrEmpty())
                return;
            
            foreach (var scalableWidget in _scalableWidgets)
            {
                scalableWidget?.AnimateWidget(enable);
            }
        }
    }
}