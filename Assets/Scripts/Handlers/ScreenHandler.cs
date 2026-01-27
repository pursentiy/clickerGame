using Common.Data.Info;
using Components.Levels;
using Extensions;
using Plugins.FSignal;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.ScreenBlocker;
using Storage.Extensions;
using UI.Screens;
using UI.Screens.ChooseLevel;
using UI.Screens.ChoosePack;
using UI.Screens.WelcomeScreen;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace Handlers
{
    public class ScreenHandler : MonoBehaviour
    {
        [Inject] private UIScreenBlocker _uiScreenBlocker;
        [Inject] private ProgressProvider _progressProvider;
        [Inject] private ProgressController _progressController;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        [Inject] private IDisposableHandlers[] _disposableHandlers;
        [Inject] private CoroutineService _coroutineService;

        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreen _chooseLevelScreen;
        [SerializeField] private WelcomeScreen _welcomeScreen;
        [SerializeField] private ChoosePackScreen _choosePackScreen;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        [SerializeField] private AnimationCurve _popupAnimationCurve;
        
        private ScreenBase _currentScreenBase;
        GameObject _backgroundGameObject;
        private const float _awaitChangeScreenTime = 0.9f;

        public float AwaitChangeScreenTime => _awaitChangeScreenTime;

        public void ShowChooseLevelScreen(PackInfo packInfo, FSignal levelResetSignal = null, bool fast = false)
        {
            var blockRef = _uiScreenBlocker.Block();
            AnimateTransition(fast).Then(() =>
            {
                levelResetSignal?.Dispatch();
                PopupCurrentScreenAndDisposeHandlers();
                var screen = Instantiate(_chooseLevelScreen, _screenCanvasTransform);
                screen.Initialize(packInfo);
                
                _currentScreenBase = screen;
                blockRef?.Dispose();
            }).CancelWith(this);
        }
        
        public void ShowChoosePackScreen(bool fast = false)
        {
            var blockRef = _uiScreenBlocker.Block();
            AnimateTransition(fast).Then(() =>
            {
                PopupCurrentScreenAndDisposeHandlers();
                _currentScreenBase = Instantiate(_choosePackScreen, _screenCanvasTransform);
                blockRef?.Dispose();
            }).CancelWith(this);;
        }

        public void ShowWelcomeScreen(bool fast = false)
        {
            var blockRef = _uiScreenBlocker.Block();
            AnimateTransition(fast).Then(() =>
            {
                PopupCurrentScreenAndDisposeHandlers();
                _currentScreenBase = Instantiate(_welcomeScreen, _screenCanvasTransform);
                blockRef?.Dispose();
            }).CancelWith(this);;
        }
        
        public void StartNewLevel(int levelId, PackInfo packInfo, LevelInfo levelInfo, bool fast = false)
        {
            var blockRef = _uiScreenBlocker.Block();
            AnimateTransition(fast).Then(() =>
            {
                _progressController.SetCurrentLevelId(levelId);
                PopupCurrentScreenAndDisposeHandlers();
                _levelSessionHandler.StartLevel(levelInfo.ToSnapshot(), _levelParamsHandler.LevelHudHandlerPrefab, packInfo);
                blockRef?.Dispose();
            }).CancelWith(this);
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
        //     
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
        
        private void OnDestroy()
        {
            if (_backgroundGameObject != null)
                Destroy(_backgroundGameObject);
        }
        
        private IPromise AnimateTransition(bool fast = false)
        {
            var particlesPromise = TryStartParticlesAwaitPromiseTransition(fast);
            var screenHidePromise = !fast ? (_currentScreenBase?.HideScreen() ?? Promise.Resolved()) : Promise.Resolved();
            var levelHudHidePromise = _levelSessionHandler.HideHUD(fast);

            return Promise.All(particlesPromise, screenHidePromise, levelHudHidePromise);
        }
        
        private IPromise TryStartParticlesAwaitPromiseTransition(bool fast)
        {
            if (fast) 
                return Promise.Resolved();
            
            PlayParticles();
            return _coroutineService.WaitFor(_awaitChangeScreenTime);
        }

        private void PlayParticles()
        {
            if (_changeScreenParticles.IsNullOrEmpty())
                return;
            
            foreach (var particles in _changeScreenParticles)
            {
                if (particles == null)
                    continue;
                
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