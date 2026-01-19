using DG.Tweening;
using Handlers;
using Services.CoroutineServices;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
        private const float AwaitTimeBeforeReload = 1f;
        
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
            // 1. Немедленно останавливаем все анимации, БЕЗ вызова OnComplete
            DOTween.KillAll(false);
    
            // 2. Сбрасываем время и звук
            Time.timeScale = 1f;
            AudioListener.pause = false;

            // 3. Очищаем сервисы
            _applicationService.DisposeServices();

            // 4. В WebGL иногда CanvasScaler сходит с ума, если его не "выключить" вручную.
            // Если у тебя есть ссылка на корневой Canvas, можно сделать:
            // Object.Destroy(canvas.GetComponent<CanvasScaler>()); 
            // Но лучше просто дать кадру завершиться.

            _persistentCoroutinesService.WaitFor(AwaitTimeBeforeReload)
                .Then(() => {
                    // Перед самой загрузкой убеждаемся, что не осталось активных операций
                    _scenesManagerService.LoadScene(SceneTypes.MainScene);
                });
        }
    }
}