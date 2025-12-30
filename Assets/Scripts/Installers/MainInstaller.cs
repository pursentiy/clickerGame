using Components.Levels;
using Handlers;
using Handlers.UISystem;
using Level.Widgets;
using Pooling;
using Services;
using Services.ContentDeliveryService;
using Storage;
using Storage.Audio;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private LevelsParamsStorage levelsParamsStorage;
        [SerializeField] private FiguresStorageData figuresStorageData;
        [SerializeField] private AudioStorageData audioStorageData;
        
        [SerializeField] private ScreenHandler _screenHandler;
        [SerializeField] private PopupHandler _popupHandler;
        [SerializeField] private LevelSessionHandler _levelSessionHandler;
        [SerializeField] private LevelParamsHandler _levelParamsHandler;
        [SerializeField] private ObjectsPoolHandler _objectsPoolHandler;
        [SerializeField] private UIBlockHandler _uiBlockHandler;
        [SerializeField] private SoundHandler _soundHandler;
        [SerializeField] private UISystemData _uiSystemData;

        private GameObject _servicesRoot;
        
        public override void InstallBindings()
        {
            _servicesRoot = new GameObject("SceneServices");
            ContainerHolder.OnProjectInstall(Container);
            
            Container.BindInterfacesAndSelfTo<ApplicationService>().AsSingle();
            
            UIInstaller.DiInstall(Container, _servicesRoot.transform, _uiSystemData);
            
            Container.BindInterfacesAndSelfTo<AddressableContentDeliveryService>().AsSingle().NonLazy();
            
            //PLAYER SERVICES
            Container.BindInterfacesAndSelfTo<GlobalSettingsService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileBuilderService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerRepositoryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileSerializerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerLevelService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerCurrencyService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelHelperService>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<CheatService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PersistentCoroutinesService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CoroutineService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelInfoTrackerService>().AsSingle().NonLazy();
            
            Container.Bind<PopupHandler>().FromInstance(_popupHandler);
            Container.Bind<ScreenHandler>().FromInstance(_screenHandler);
            Container.BindInterfacesAndSelfTo<LevelSessionHandler>().FromInstance(_levelSessionHandler);
            Container.Bind<LevelParamsHandler>().FromInstance(_levelParamsHandler);
            Container.Bind<UIBlockHandler>().FromInstance(_uiBlockHandler);
            Container.Bind<ObjectsPoolHandler>().FromInstance(_objectsPoolHandler);
            Container.BindInterfacesAndSelfTo<SoundHandler>().FromInstance(_soundHandler);
            Container.Bind<LevelsParamsStorage>().FromNewScriptableObject(levelsParamsStorage).AsTransient().NonLazy();
            Container.Bind<FiguresStorageData>().FromScriptableObject(figuresStorageData).AsSingle().NonLazy();
            Container.Bind<AudioStorageData>().FromScriptableObject(audioStorageData).AsSingle().NonLazy();
        }
    }
}