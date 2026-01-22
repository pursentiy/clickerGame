using Components.Levels;
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
        [SerializeField] private ScreenHandler _screenHandler;
        [SerializeField] private LevelSessionHandler _levelSessionHandler;
        [SerializeField] private LevelParamsHandler _levelParamsHandler;
        [SerializeField] private UIBlockHandler _uiBlockHandler;
        [SerializeField] private UISystemData _uiSystemData;

        private GameObject _servicesRoot;

        public override void InstallBindings()
        {
            var sceneServicesRoot = new GameObject("SceneServices").transform;
            
            ContainerHolder.SetCurrentContainer(Container);

            UIInstaller.DiInstall(ContainerHolder.CurrentContainer, sceneServicesRoot, _uiSystemData);
            
            Container.Bind<UIBlockHandler>().FromInstance(_uiBlockHandler).AsSingle();
            Container.Bind<ScreenHandler>().FromInstance(_screenHandler).AsSingle();
            
            Container.BindInterfacesAndSelfTo<LevelSessionHandler>().FromInstance(_levelSessionHandler).AsSingle();
            Container.Bind<LevelParamsHandler>().FromInstance(_levelParamsHandler).AsSingle();
            
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