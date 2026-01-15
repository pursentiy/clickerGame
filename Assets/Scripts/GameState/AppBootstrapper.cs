using System.Collections;
using Installers;
using Playgama;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameState
{
    public class AppBootstrapper: InjectableMonoBehaviour
    {
        [Inject] private readonly LocalizationService _localizationService;
        
        [Header("Settings")]
        [SerializeField] private string _nextSceneName = "1_MainScene";
        
        private bool _isBridgeAuthComplete = false;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private IEnumerator Start()
        {
            LoggerService.LogWarning($"[{GetType().Name}] Initialization started...");
            
            yield return BridgeAuthenticationRoutine();
            yield return LocalizationPreloadRoutine();

            LoggerService.LogWarning($"[{GetType().Name}] Initialization finished...");
            
            SceneManager.LoadScene(_nextSceneName);
        }
        
        private IEnumerator BridgeAuthenticationRoutine()
        {
            while (Bridge.platform.id == "unknown")
            {
                yield return null;
            }

            LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Платформа определена: {Bridge.platform.id}");
            
            Bridge.player.Authorize(null, success =>
            {
                if (success)
                {
                    LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Авторизация успешна");
                }
                else
                {
                    LoggerService.LogWarning($"[{GetType().Name}] [{nameof(BridgeAuthenticationRoutine)}] Вход в режиме гостя");
                }
                
                _isBridgeAuthComplete = true;
            });
            
            while (!_isBridgeAuthComplete)
            {
                yield return null;
            }
        }
        
        private IEnumerator LocalizationPreloadRoutine()
        {
            LoggerService.LogWarning($"[{GetType().Name}] [{nameof(LocalizationPreloadRoutine)}]: Waiting for Localization...");
            yield return _localizationService.InitializeRoutine();

            // Проверка на выживание объекта после долгих ожиданий
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