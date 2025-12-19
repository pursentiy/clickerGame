using System;
using Handlers.UISystem.Popups;
using UnityEngine;
using Zenject;

namespace Handlers.UISystem
{
    public static class UIInstaller
    {
        private static DiContainer _lastContainer;

        public static void DiInstall(DiContainer container, Transform serviceRoot, UISystemData uiSystemData)
        {
            container.BindInterfacesAndSelfTo<UIManager>().FromNewComponentOnNewGameObject().WithGameObjectName("UIManager")
                .UnderTransform(serviceRoot).AsSingle().NonLazy();
            container.Bind<IUIView>().FromComponentSibling().AsTransient();
            container.Bind<UISystemData>().FromScriptableObject(uiSystemData).AsSingle();
            container.BindInterfacesAndSelfTo<UIPopupsHandler>().FromNew().AsSingle();
            container.Bind<UIPopupBase>().ToSelf().FromComponentSibling().AsTransient();
            _lastContainer = container;
        }

        public static DiContainer Container
        {
            get
            {
                if (_lastContainer == null)
                {
                    throw new Exception("No UI System installed!");
                }
            
                return _lastContainer;
            }
        }
    }
}