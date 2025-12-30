using System.Collections;
using Components.Levels;
using Extensions;
using Installers;
using Plugins.FSignal;
using RSG;
using Screen.ChooseLevel;
using Screen.ChoosePack;
using Screen.WelcomeScreen;
using Services;
using Storage.Extensions;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class ScreenHandler : InjectableMonoBehaviour
    {
        [Inject] private UIBlockHandler _uiBlockHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        [Inject] private IDisposableHandlers[] _disposableHandlers;

        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreen _chooseLevelScreen;
        [SerializeField] private WelcomeScreen _welcomeScreen;
        [SerializeField] private ChoosePackScreen _choosePackScreen;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        [SerializeField] private AnimationCurve _popupAnimationCurve;
        
        private Screen.ScreenBase _currentScreenBase;
        private const float _awaitChangeScreenTime = 0.7f;

        public float AwaitChangeScreenTime => _awaitChangeScreenTime;

        public void ShowChooseLevelScreen(FSignal levelResetSignal = null, bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                levelResetSignal?.Dispatch();
                PopupCurrentScreenAndDisposeHandlers();
                _currentScreenBase = Instantiate(_chooseLevelScreen, _screenCanvasTransform);
            });
        }
        
        public void ShowChoosePackScreen(bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                PopupCurrentScreenAndDisposeHandlers();
                _currentScreenBase = Instantiate(_choosePackScreen, _screenCanvasTransform);
            });
        }

        public void ShowWelcomeScreen(bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);
            
            awaitPromise.Then(() =>
            {
                PopupCurrentScreenAndDisposeHandlers();
                _currentScreenBase = Instantiate(_welcomeScreen, _screenCanvasTransform);
            });
        }

        private void PopupCurrentScreenAndDisposeHandlers()
        {
            PopupAllScreenHandlers();
            DisposeAllHandlers();
        }

        private void PopupAllScreenHandlers()
        {
            if (_currentScreenBase == null)
            {
                return;
            }
            
            Destroy(_currentScreenBase.gameObject);
            _currentScreenBase = null;
        }

        private void DisposeAllHandlers()
        {
            if (_disposableHandlers.IsNullOrEmpty())
                return;

            foreach (var disposableHandler in _disposableHandlers)
            {
                disposableHandler.Dispose();
            }
        }

        //TODO ADD REPLAY LEVEL LOGIC
        // public void ReplayCurrentLevel(int levelNumber, bool fast = false)
        // {
        //     var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);
        //
        //     awaitPromise.Then(() =>
        //     {
        //         _playerProgressService.ResetLevelProgress(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber);
        //         var levelParams = _playerProgressService.GetLevelByNumber(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber);
        //         _playerProgressService.CurrentLevelNumber = levelNumber;
        //         PopupCurrentScreenAndDisposeHandlers();
        //         _levelSessionHandler.StartLevel(levelParams, _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
        //     });
        //}

        public void StartNewLevel(int levelNumber, LevelParams levelParams, bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                _playerProgressService.CurrentLevelNumber = levelNumber;
                PopupCurrentScreenAndDisposeHandlers();
                _levelSessionHandler.StartLevel(levelParams.ToSnapshot(), _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
            });
        }
        
        private IPromise TryStartParticlesAwaitPromiseTransition(bool fast)
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