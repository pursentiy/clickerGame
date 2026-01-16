using System.Collections;
using Installers;
using Playgama;
using Services;
using UnityEngine;
using Zenject;

namespace GameState
{
    public class AppBootstrapper: InjectableMonoBehaviour
    {
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScenesManagerService _scenesManagerService;
        
#if UNITY_EDITOR
        [SerializeField] private bool _skipNextSceneLoading = false;
#endif
        
        private bool _isBridgeAuthComplete = false;
        
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
#if UNITY_EDITOR
            if (_skipNextSceneLoading)
            {
                LoggerService.LogWarning($"!!!ALERT: {nameof(AppBootstrapper)}: {nameof(_skipNextSceneLoading)} field is set to true, so you will not proceed to the next scene!!!");
            }
#endif
        }
        
        private IEnumerator Start()
        {
            LoggerService.LogWarning($"[{GetType().Name}] Initialization started...");
            
            yield return BridgeAuthenticationRoutine();
            yield return LocalizationPreloadRoutine();

            LoggerService.LogWarning($"[{GetType().Name}] Initialization finished...");
            
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
            while (Bridge.platform.id == "unknown")
            {
                yield return null;
            }

            LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Platform detected: {Bridge.platform.id}");
            
            while (Bridge.platform.id == "unknown") yield return null;
            
#if UNITY_EDITOR
            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Editor detected: Automatic authorization bypass");
            _isBridgeAuthComplete = true;
#else
        Bridge.player.Authorize(null, success =>
            {
                if (success)
                {
                    LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Successful player authorization.");
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
            LoggerService.LogWarning($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Waiting for Localization...");
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
            
            LoggerService.LogWarning($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Successfully loaded Localization.");
        }
    }
}