using System.Collections;
using Common;
using Installers;
using Playgama;
using Services;
using UnityEngine;
using Zenject;

namespace GameState
{
    public class AppBootstrapper: InjectableMonoBehaviour
    {
        private const float BridgePlatformAwaitTimeout = 5f;
        
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScenesManagerService _scenesManagerService;
        [Inject] private readonly GlobalSettingsManager _globalSettingsManager;
        
#if UNITY_EDITOR
        [SerializeField] private bool _skipNextSceneLoading = false;
#endif
        
        private bool _isBridgeAuthComplete = false;
        
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            _globalSettingsManager.SetMaxFrameRate();
            _globalSettingsManager.DisableMultitouch();
            
#if UNITY_EDITOR
            if (_skipNextSceneLoading)
            {
                LoggerService.LogWarning($"!!!ALERT: {nameof(AppBootstrapper)}: {nameof(_skipNextSceneLoading)} field is set to true, so you will not proceed to the next scene!!!");
            }
#endif
        }
        
        private IEnumerator Start()
        {
            LoggerService.LogDebug($"[{GetType().Name}] Initialization started...");
            
            yield return BridgeAuthenticationRoutine();
            yield return LocalizationPreloadRoutine();

            LoggerService.LogDebug($"[{GetType().Name}] Initialization finished...");
            
#if UNITY_EDITOR
            if (!_skipNextSceneLoading)
            {
                LoadNextScene();
            }
#else
            LoadNextScene();
#endif
        }

        private void LoadNextScene()
        {
            _scenesManagerService.LoadScene(SceneTypes.MainScene);
        }
        
        private IEnumerator BridgeAuthenticationRoutine()
        {
            var timeout = BridgePlatformAwaitTimeout; 
            while (Bridge.platform.id == GlobalConstants.BridgeUnknown && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            LoggerService.LogDebug($"Platform detected: {Bridge.platform.id}");

            if (Bridge.platform.id == GlobalConstants.BridgeGDId)
            {
                _isBridgeAuthComplete = true;
                yield break;
            }

            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Platform detected: {Bridge.platform.id}");
            
            while (Bridge.platform.id == GlobalConstants.BridgeUnknown) yield return null;
            
#if UNITY_EDITOR
            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Editor detected: Automatic authorization bypass");
            _isBridgeAuthComplete = true;
#else
        Bridge.player.Authorize(null, success =>
            {
                if (success)
                {
                    LoggerService.LogDebug($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Successful player authorization.");
                }
                else
                {
                    LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Guest mode.");
                }
                
                _isBridgeAuthComplete = true;
            });
#endif

            while (!_isBridgeAuthComplete)
            {
                yield return null;
            }
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
    }
}