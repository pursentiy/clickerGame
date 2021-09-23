using System;
using System.Collections;
using DG.Tweening;
using Installers;
using RSG;
using Screen;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class ScreenHandler : InjectableMonoBehaviour, IScreenHandler
    {
        [Inject] private UIBlockHandler _uiBlockHandler;
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        
        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreenBase _chooseLevelScreenBase;
        [SerializeField] private WelcomeScreenBase _welcomeScreenBase;
        [SerializeField] private ChoosePackScreenBase _choosePackScreenBase;
        [SerializeField] private LevelCompleteScreenBase _levelCompleteScreenBase;
        [SerializeField] private ParticleSystem[] _changeScreenParticles;
        [SerializeField] private AnimationCurve _popupAnimationCurve;
        
        private Screen.ScreenBase _currentScreenBase;
        private const float _awaitChangeScreenTime = 0.7f;

        public float AwaitChangeScreenTime => _awaitChangeScreenTime;

        public void ShowChooseLevelScreen(bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_chooseLevelScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowChoosePackScreen(bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_choosePackScreenBase, _screenCanvasTransform);
            });
        }

        public void ShowWelcomeScreen(bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);
            
            awaitPromise.Then(() =>
            {
                PopupAllScreenHandlers();
                _currentScreenBase = Instantiate(_welcomeScreenBase, _screenCanvasTransform);
            });
        }
        
        public void ShowLevelCompleteScreen(Camera sourceCamera, bool onLevelEnter, Action onFinishAction, Sprite figureSprite, bool fast = false)
        {
            _uiBlockHandler.BlockUserInput(true);
            var screenHandler = Instantiate(_levelCompleteScreenBase, _screenCanvasTransform);
            screenHandler.gameObject.SetActive(false);
            screenHandler.SetupFigureImage(figureSprite);
            screenHandler.SetBackgroundFigure();
            
            DOTween.Sequence().Append(screenHandler.PopupTransform.DOScale(new Vector3(0, 0, 0), 0.01f))
                .Join(screenHandler.ScreenTransform.DOScale(new Vector3(0, 0, 0), 0.01f))
                .AppendCallback(() =>
                {
                    screenHandler.gameObject.SetActive(true);
                })
                .Append(screenHandler.ScreenTransform.DOScale(new Vector3(1f, 1f, 1f), 0.75f).SetEase(_popupAnimationCurve))
                .Insert(0.1f,screenHandler.PopupTransform.DOScale(new Vector3(1f, 1f, 1f), 0.8f).SetEase(_popupAnimationCurve))
                .OnComplete(() =>
            {
                screenHandler.SetOnFinishLevelSessionAction(onFinishAction);

                if(!onLevelEnter)
                    screenHandler.PlayFireworksParticles();
                
                _uiBlockHandler.BlockUserInput(false);
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

        public void ReplayCurrentLevel(int levelNumber, bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                _progressHandler.ResetLevelProgress(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber);
                var levelParams = _progressHandler.GetLevelByNumber(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber);
                _progressHandler.CurrentLevelNumber = levelNumber;
                PopupAllScreenHandlers();
                _levelSessionHandler.StartLevel(levelParams, _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
            });
        }

        public void StartNewLevel(int levelNumber, LevelParams levelParams, bool fast = false)
        {
            var awaitPromise = TryStartParticlesAwaitPromiseTransition(fast);

            awaitPromise.Then(() =>
            {
                _progressHandler.CurrentLevelNumber = levelNumber;
                PopupAllScreenHandlers();
                _levelSessionHandler.StartLevel(levelParams, _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
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