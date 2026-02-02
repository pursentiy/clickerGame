using System.Collections;
using Extensions;
using Installers;
using RSG;
using Services;
using Utilities.Disposable;
using Zenject;

namespace GameState
{
    public class AppBootstrapper: InjectableMonoBehaviour
    {
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScenesManagerService _scenesManagerService;
        [Inject] private readonly GlobalSettingsManager _globalSettingsManager;
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly BridgeService _bridgeService;
        
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            _globalSettingsManager.SetMaxFrameRate();
            _globalSettingsManager.DisableMultitouch();
        }
        
        private IEnumerator Start()
        {
            LoggerService.LogDebug($"[{GetType().Name}] Initialization started...");
            
            yield return _bridgeService.PlatformDetectionRoutine();
            yield return LocalizationPreloadRoutine();

            LoggerService.LogDebug($"[{GetType().Name}] Initialization finished...");

            TryAuthenticatePlayer()
                .Then(TryShowPrerollInterstitialAd)
                .ContinueWithResolved(LoadNextScene)
                .CancelWith(this);
        }
        
        private IEnumerator LocalizationPreloadRoutine()
        {
            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Waiting for Localization...");
            yield return _localizationService.InitializeRoutine();
            
            if (this == null)
            {
                yield break;
            }

            if (!_localizationService.IsInitialized)
            {
                LoggerService.LogError($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Localization failed. Stopping.");
                yield break;
            }
            
            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Successfully loaded Localization.");
        }

        private IPromise TryAuthenticatePlayer()
        {
            if (!_bridgeService.WasAuthorizedBefore)
                return Promise.Resolved();
            
            return _bridgeService.AuthenticatePlayer();
        }
        
        private IPromise TryShowPrerollInterstitialAd()
        {
            if (!_adsService.CanShowPrerollAd())
                return Promise.Resolved();
            
            return _adsService.ShowInterstitial().AsNonGenericPromise().CancelWith(this);
        }
        
        private void LoadNextScene()
        {
            _scenesManagerService.LoadScene(SceneTypes.MainScene);
        }
    }
}