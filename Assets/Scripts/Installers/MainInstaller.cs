using Handlers;
using Pooling;
using Services;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private LevelsParamsStorage levelsParamsStorage;
        [SerializeField] private FiguresStorage _figuresStorage;
        
        [SerializeField] private ProgressHandler _progressHandler;
        [SerializeField] private ScreenHandler _screenHandler;
        [SerializeField] private LevelSessionHandler _levelSessionHandler;
        [SerializeField] private LevelParamsHandler _levelParamsHandler;
        [SerializeField] private ObjectsPoolHandler _objectsPoolHandler;

        public override void InstallBindings()
        {
            ContainerHolder.OnProjectInstall(Container);

            Container.Bind<ProgressHandler>().FromInstance(_progressHandler);
            Container.Bind<ScreenHandler>().FromInstance(_screenHandler);
            Container.Bind<LevelSessionHandler>().FromInstance(_levelSessionHandler);
            Container.Bind<LevelParamsHandler>().FromInstance(_levelParamsHandler);
            Container.Bind<ObjectsPoolHandler>().FromInstance(_objectsPoolHandler);
            Container.Bind<LevelsParamsStorage>().FromNewScriptableObject(levelsParamsStorage).AsTransient().NonLazy();
            Container.Bind<FiguresStorage>().FromScriptableObject(_figuresStorage).AsSingle().NonLazy();
            Container.Bind<IProcessProgressDataService>().To<ProcessProgressDataService>().AsSingle().NonLazy();
        }
    }
}