using Common.Rewards;
using Handlers;
using Services;
using Services.ContentDeliveryService;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.Player;
using Storage;
using Storage.Audio;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private CurrencyLibrary _currencyLibrary;
        [SerializeField] private AudioStorageData _audioStorageData;
        [SerializeField] private SoundHandler _soundHandler;
        [SerializeField] private LevelsParamsStorageData _levelsParamsStorageData;

        public override void InstallBindings()
        {
            // INFRASTRUCTURE AND CORE SERVICES
            Container.BindInterfacesAndSelfTo<ApplicationService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScenesManagerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PersistentCoroutinesService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressableContentDeliveryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReloadService>().AsSingle();

            // DATA AND PLAYER PROGRESS
            Container.BindInterfacesAndSelfTo<ProfileStorageService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerRepositoryService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerProfileManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerCurrencyService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProgressProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProgressController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileBuilderService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameSettingsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LanguageConversionService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameConfigurationProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameParamsManager>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<SoundHandler>().FromInstance(_soundHandler).AsSingle();
            Container.BindInterfacesAndSelfTo<LevelsParamsStorageData>().FromScriptableObject(_levelsParamsStorageData).AsSingle();

            // STATIC DATA
            Container.Bind<CurrencyLibrary>().FromScriptableObject(_currencyLibrary).AsSingle();
            Container.Bind<AudioStorageData>().FromScriptableObject(_audioStorageData).AsSingle();
            Container.BindInterfacesAndSelfTo<CurrencyLibraryService>().AsSingle();
        }
    }
}