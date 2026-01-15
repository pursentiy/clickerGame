using Components.Levels;
using Handlers;
using Handlers.UISystem;
using Level.Widgets;
using Services;
using Services.FlyingRewardsAnimation;
using Storage;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using Services.Cheats;
#endif

namespace Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private LevelsParamsStorageData _levelsParamsStorageData;
        [SerializeField] private ScreenHandler _screenHandler;
        [SerializeField] private PopupHandler _popupHandler;
        [SerializeField] private LevelSessionHandler _levelSessionHandler;
        [SerializeField] private LevelParamsHandler _levelParamsHandler;
        [SerializeField] private UIScreenUpdater _uiScreenUpdater;
        [SerializeField] private UIBlockHandler _uiBlockHandler;
        [SerializeField] private SoundHandler _soundHandler;
        [SerializeField] private UISystemData _uiSystemData;

        private GameObject _servicesRoot;

        public override void InstallBindings()
        {
            var sceneServicesRoot = new GameObject("SceneServices").transform;

            UIInstaller.DiInstall(Container, sceneServicesRoot, _uiSystemData);
            
            Container.BindInterfacesAndSelfTo<CoroutineService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickHandlerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelHelperService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelInfoTrackerService>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<FlyingUIRewardAnimationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<FlyingUIRewardDestinationService>().AsSingle();
            
            Container.Bind<ScreenHandler>().FromInstance(_screenHandler).AsSingle();
            Container.Bind<PopupHandler>().FromInstance(_popupHandler).AsSingle();
            Container.Bind<UIBlockHandler>().FromInstance(_uiBlockHandler).AsSingle();
            Container.Bind<UIScreenUpdater>().FromInstance(_uiScreenUpdater).AsSingle();

            Container.BindInterfacesAndSelfTo<LevelSessionHandler>().FromInstance(_levelSessionHandler).AsSingle();
            Container.Bind<LevelParamsHandler>().FromInstance(_levelParamsHandler).AsSingle();
            Container.BindInterfacesAndSelfTo<SoundHandler>().FromInstance(_soundHandler).AsSingle();
            Container.Bind<LevelsParamsStorageData>().FromScriptableObject(_levelsParamsStorageData).AsSingle();
            
#if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<CheatService>().AsSingle().NonLazy();
#endif
        }
    }
}