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
        [SerializeField] private ChoosePackScreenBase _choosePackScreenBase;
        [SerializeField] private LevelCompleteScreenBase _levelCompleteScreenBase;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        
        private Screen.ScreenBase _currentScreenBase;
        private const float _awaitChangeScreenTime = 1f;

        public void ShowChooseLevelScreen(bool fast = false)
        {
            var awaitPromise = TryStartAwaitPromise(fast);

            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_chooseLevelScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowChoosePackScreen(bool fast = false)
        {
            var awaitPromise = TryStartAwaitPromise(fast);

            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_choosePackScreenBase, _screenCanvasTransform);
            });
        }

        public void ShowWelcomeScreen(bool fast = false)
        {
            var awaitPromise = TryStartAwaitPromise(fast);
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_welcomeScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowLevelCompleteScreen(Camera sourceCamera, Action onFinishAction, bool fast = false)
        {
            var awaitPromise = TryStartAwaitPromise(fast);
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();

                var screenHandler = Instantiate(_levelCompleteScreenBase, _screenCanvasTransform);
                screenHandler.SetOnFinishLevelSessionAction(onFinishAction);
                screenHandler.SetupTextureCamera(sourceCamera);
            
                _currentScreenBase = screenHandler;
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
        
        private IPromise TryStartAwaitPromise(bool fast)
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

            return awaitPromise;
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