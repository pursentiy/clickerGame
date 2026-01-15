using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
        [Inject] private readonly ZenjectSceneLoader _sceneLoader;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly CoroutineService _coroutineService;
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

            _coroutineService.WaitFrames(3)
                .Then(Finally);
            
            void Finally()
            {
                _scenesManagerService.LoadScene(SceneTypes.MainScene);
            }
        }
    }
}