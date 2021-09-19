using System;
using System.Collections;
using Installers;
using Screen;
using UnityEngine;

namespace Handlers
{
    public class ScreenHandler : InjectableMonoBehaviour, IScreenHandler
    {
        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreenBase _chooseLevelScreenBase;
        [SerializeField] private WelcomeScreenBase _welcomeScreenBase;
        [SerializeField] private LevelCompleteScreenBase _levelCompleteScreenBase;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        
        private Screen.ScreenBase _currentScreenBase;
        private const float _awaitChangeScreenTime = 1f;

        public void ShowChooseLevelScreen()
        {
            PopupAllScreenHandlers();

            _currentScreenBase = Instantiate(_chooseLevelScreenBase, _screenCanvasTransform);
        }
        
        public void ShowWelcomeScreen()
        {
            PopupAllScreenHandlers();

            _currentScreenBase = Instantiate(_welcomeScreenBase, _screenCanvasTransform);
        }
        
        public void ShowLevelCompleteScreen(int currentLevel, Camera sourceCamera, Action onFinishAction)
        {
            PopupAllScreenHandlers();

            var screenHandler = Instantiate(_levelCompleteScreenBase, _screenCanvasTransform);
            screenHandler.SetOnFinishLevelSessionAction(onFinishAction);
            screenHandler.SetupTextureCamera(sourceCamera);
            
            _currentScreenBase = screenHandler;
            _currentScreenBase.CurrentLevel = currentLevel;
        }

        public void PopupAllScreenHandlers()
        {
            if (_currentScreenBase == null)
            {
                return;
            }
            
            Destroy(_currentScreenBase.gameObject);
            _currentScreenBase = null;
        }

        private IEnumerator AwaitBeforeChangingScreen()
        {
            yield return new WaitForSeconds(_awaitChangeScreenTime);
        }
        
        private void PlayParticles(ParticleSystem[] particlesToPlay)
        {
            foreach (var particles in particlesToPlay)
            {
                if (particles.isPlaying)
                {
                    particles.Stop();
                }

                particles.Simulate(0);
                particles.Play();
            }
        }
    }
}