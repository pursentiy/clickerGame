using DG.Tweening;
using Services.CoroutineServices;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
        private const float AwaitTimeBeforeReload = 0.5f;
        
        [Inject] private readonly ZenjectSceneLoader _sceneLoader;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly PersistentCoroutinesService _persistentCoroutinesService;
        [Inject] private readonly ScenesManagerService _scenesManagerService;

        public void SoftRestart()
        {
            RestartRoutine();
        }

        private void RestartRoutine()
        {
            DOTween.KillAll(false);
    
            Time.timeScale = 1f;
            AudioListener.pause = false;

            _applicationService.DisposeServices();

            _persistentCoroutinesService.WaitFor(AwaitTimeBeforeReload)
                .Then(() => {
                    // Перед самой загрузкой убеждаемся, что не осталось активных операций
                    _scenesManagerService.LoadScene(SceneTypes.MainScene);
                });
        }
    }
}