using Controllers;
using Handlers;
using Handlers.UISystem;
using Level.Widgets;
using Services;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using Services.Cheats;
#endif

namespace Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private UISystemData _uiSystemData;
        [SerializeField] private ScreenTransitionParticlesHandler _screenTransitionParticlesHandler;

        private GameObject _servicesRoot;

        public override void InstallBindings()
        {
            var sceneServicesRoot = new GameObject("SceneServices").transform;
            
            ContainerHolder.SetCurrentContainer(Container);

            UIInstaller.DiInstall(ContainerHolder.CurrentContainer, sceneServicesRoot, _uiSystemData);

            Container.BindInterfacesAndSelfTo<ScreenTransitionParticlesHandler>().FromComponentInNewPrefab(_screenTransitionParticlesHandler).WithGameObjectName("[ScreenTransition]")
                .UnderTransform(sceneServicesRoot).AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<FlowScreenController>().AsSingle();
            Container.BindInterfacesAndSelfTo<FlowPopupController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickHandlerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelHelperService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelInfoTrackerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CoroutineService>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<FlyingUIRewardAnimationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<FlyingUIRewardDestinationService>().AsSingle();
            
#if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<CheatService>().AsSingle().NonLazy();
#endif
        }
    }
}