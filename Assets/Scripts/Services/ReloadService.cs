using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
        [Inject] private ZenjectSceneLoader _sceneLoader;
        [Inject] private ApplicationService _applicationService;
        [Inject] private CoroutineService _coroutineService;

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

            _coroutineService.WaitFrame()
                .Then(Finally);
            
            void Finally()
            {
                _sceneLoader.LoadScene(0);
            }
        }
    }
}