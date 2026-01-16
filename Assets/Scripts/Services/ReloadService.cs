using DG.Tweening;
using Services.CoroutineServices;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
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
            DOTween.KillAll();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            _applicationService.DisposeServices();

            _persistentCoroutinesService.WaitFrames(3)
                .Then(Finally);
            
            void Finally()
            {
                _scenesManagerService.LoadScene(SceneTypes.MainScene);
            }
        }
    }
}