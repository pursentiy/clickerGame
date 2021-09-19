using System;
using System.Collections;
using Installers;
using RSG;
using Screen;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class ScreenHandler : InjectableMonoBehaviour, IScreenHandler
    {
        [Inject] private UIBlockHandler _uiBlockHandler;
        
        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreenBase _chooseLevelScreenBase;
        [SerializeField] private WelcomeScreenBase _welcomeScreenBase;
        [SerializeField] private LevelCompleteScreenBase _levelCompleteScreenBase;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        
        private Screen.ScreenBase _currentScreenBase;
        private const float _awaitChangeScreenTime = 1f;

        public void ShowChooseLevelScreen(bool fast = false)
        {
            IPromise awaitPromise;

            if (!fast)
            {
                awaitPromise = StartAwaitCoroutine();
                PlayParticles();
            }
            else
            {
                awaitPromise = Promise.Resolved();
            }    
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_chooseLevelScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowWelcomeScreen(bool fast = false)
        {
            IPromise awaitPromise;

            if (!fast)
            {
                awaitPromise = StartAwaitCoroutine();
                PlayParticles();
            }
            else
            {
                awaitPromise = Promise.Resolved();
            }  
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_welcomeScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowLevelCompleteScreen(int currentLevel, Camera sourceCamera, Action onFinishAction, bool fast = false)
        {
            IPromise awaitPromise;

            if (!fast)
            {
                awaitPromise = StartAwaitCoroutine();
                PlayParticles();
            }
            else
            {
                awaitPromise = Promise.Resolved();
            }  
            
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();

                var screenHandler = Instantiate(_levelCompleteScreenBase, _screenCanvasTransform);
                screenHandler.SetOnFinishLevelSessionAction(onFinishAction);
                screenHandler.SetupTextureCamera(sourceCamera);
            
                _currentScreenBase = screenHandler;
                _currentScreenBase.CurrentLevel = currentLevel;
            });
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
        
        private IPromise StartAwaitCoroutine()
        {
            var awaitPromise = new Promise();
            StartCoroutine(AwaitBeforeChangingScreen(awaitPromise));
            return awaitPromise;
        }

        private IEnumerator AwaitBeforeChangingScreen(Promise awaitPromise)
        {
            _uiBlockHandler.BlockUserInput(true);
            yield return new WaitForSeconds(_awaitChangeScreenTime);
            
            _uiBlockHandler.BlockUserInput(false);
             if(awaitPromise.CurState == PromiseState.Pending)
                 awaitPromise.Resolve();
        }
        
        private void PlayParticles()
        {
            foreach (var particles in _changeScreenParticles)
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