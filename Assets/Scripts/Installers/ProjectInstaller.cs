using Common.Rewards;
using Services;
using Services.ContentDeliveryService;
using Services.FlyingRewardsAnimation;
using Storage.Audio;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private CurrencyLibrary _currencyLibrary;
        [SerializeField] private AudioStorageData _audioStorageData;

        public override void InstallBindings()
        {
            ContainerHolder.OnProjectInstall(Container);
            
            // INFRASTRUCTURE AND CORE SERVICES
            Container.BindInterfacesAndSelfTo<ApplicationService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScenesManagerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CoroutineService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PersistentCoroutinesService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressableContentDeliveryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReloadService>().AsSingle();

            // DATA AND PLAYER PROGRESS
            Container.BindInterfacesAndSelfTo<ProfileStorageService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerRepositoryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerCurrencyService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerProgressService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileBuilderService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GlobalSettingsService>().AsSingle();

            // STATIC DATA
            Container.Bind<CurrencyLibrary>().FromScriptableObject(_currencyLibrary).AsSingle();
            Container.Bind<AudioStorageData>().FromScriptableObject(_audioStorageData).AsSingle();
            Container.BindInterfacesAndSelfTo<CurrencyLibraryService>().AsSingle();
        }
    }
}