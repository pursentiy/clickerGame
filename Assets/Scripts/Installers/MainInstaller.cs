using Handlers;
using Level.Widgets;
using Pooling;
using Services;
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

        public override void InstallBindings()
        {
            ContainerHolder.OnProjectInstall(Container);
            
            //PLAYER SERVICES
            Container.BindInterfacesAndSelfTo<PlayerSnapshotService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileBuilderService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerRepositoryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileSerializerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerProgressService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerCurrencyService>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<CheatService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PersistentCoroutinesService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CoroutineService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelInfoTrackerService>().AsSingle().NonLazy();
            
            Container.Bind<PopupHandler>().FromInstance(_popupHandler);
            Container.Bind<ScreenHandler>().FromInstance(_screenHandler);
            Container.Bind<LevelSessionHandler>().FromInstance(_levelSessionHandler);
            Container.Bind<LevelParamsHandler>().FromInstance(_levelParamsHandler);
            Container.Bind<UIBlockHandler>().FromInstance(_uiBlockHandler);
            Container.Bind<ObjectsPoolHandler>().FromInstance(_objectsPoolHandler);
            Container.Bind<SoundHandler>().FromInstance(_soundHandler);
            Container.Bind<LevelsParamsStorage>().FromNewScriptableObject(levelsParamsStorage).AsTransient().NonLazy();
            Container.Bind<FiguresStorageData>().FromScriptableObject(figuresStorageData).AsSingle().NonLazy();
            Container.Bind<AudioStorageData>().FromScriptableObject(audioStorageData).AsSingle().NonLazy();
        }
    }
}