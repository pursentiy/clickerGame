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
            //_uiBlockHandler.BlockUserInput(true);
            DOTween.KillAll();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            _applicationService.DisposeServices();

            //TODO UPDATE AWAIT LOGIC
            _persistentCoroutinesService.WaitFor(AwaitTimeBeforeReload)
                .Then(Finally);
            
            void Finally()
            {
                _scenesManagerService.LoadScene(SceneTypes.MainScene);
            }
        }
    }
}