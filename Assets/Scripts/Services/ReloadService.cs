using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ReloadService
    {
        [Inject] private ZenjectSceneLoader _sceneLoader;
        [Inject] private ApplicationService _applicationService;
        
        public void SoftRestart()
        {
            DOTween.KillAll(); 
            AudioListener.pause = false; // На случай если игра была на паузе
            Time.timeScale = 1f;
            _applicationService.DisposeServices();

            _sceneLoader.LoadScene(0);
        }
    }
}