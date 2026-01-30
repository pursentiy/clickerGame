using System.Collections.Generic;
using Extensions;
using Installers;
using Services.ScreenObserver;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace Common.Widgets.ContainerScaler
{
    public class UIScalerHandler : InjectableMonoBehaviour
    {
        [Inject] private ScreenObserverService _screenObserverService;
        
        [SerializeField] private GameObject[] _scalableWidgetsGameObjects;
        
        private List<IScalableWidget> _scalableWidgets = new ();
        private bool _canUpdate = true;

        protected override void Awake()
        {
            base.Awake();

            _screenObserverService.OnOrientationChangeSignal.MapListener(OnOrientationChanged).DisposeWith(this);
            _screenObserverService.OnResolutionChangeSignal.MapListener(OnResolutionChanged).DisposeWith(this);
            
            GetScalableWidgets();
        }

        private void OnOrientationChanged(ScreenOrientation orientation)
        {
            TryUpdateWidgets();
        }

        private void OnResolutionChanged()
        {
            TryUpdateWidgets();
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

        private void TryUpdateWidgets()
        {
            if (!_canUpdate || _scalableWidgets.IsCollectionNullOrEmpty())
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