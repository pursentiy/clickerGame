using System;
using System.Collections;
using Animations;
using Handlers;
using Static;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Screen
{
    public class LevelCompleteScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Image _completeFigureImage;
        [SerializeField] private RectTransform _popupTransform;
        [SerializeField] private RectTransform _screenTransform;
        [SerializeField] private ParticleSystem[] _fireworksParticles;
        [SerializeField] private ScreenColorAnimation _screenColorAnimation;

        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Action _onFinishLevelSessionAction;

        public RectTransform PopupTransform => _popupTransform;
        public RectTransform ScreenTransform => _screenTransform;

        public void StartColorAnimationLoop(Gradient colorGradient)
        {
            _screenColorAnimation.StartColorLoop(colorGradient);
        }

        public void SetOnFinishLevelSessionAction(Action action)
        {
            _onFinishLevelSessionAction = action;
        }

        public void SetupFigureImage(Sprite completedFigureSprite)
        {
            _completeFigureImage.sprite = completedFigureSprite;
        }

        public void PlayFireworksParticles()
        {
            var fireworksParticlesArray = new int[_fireworksParticles.Length];
            for (var i = 0; i < _fireworksParticles.Length; i++)
            {
                fireworksParticlesArray[i] = i;
            }

            StartCoroutine(PlayParticles(Extensions.Shuffle(fireworksParticlesArray)));
        }

        private IEnumerator PlayParticles(int[] shuffledPositions)
        {
            foreach (var fireworkPosition in shuffledPositions)
            {
                yield return new WaitForSeconds(0.1f);
                _fireworksParticles[fireworkPosition].Play();
            }
            
            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
            PlayFireworksParticles();
        }

        private void Start()
        {
            InitializeLevelsButtons();
        }

        private void InitializeLevelsButtons()
        {
            _backButton.onClick.AddListener(()=> _screenHandler.ShowChooseLevelScreen());
            _replayButton.onClick.AddListener(TryAgainLevel);
        }

        private void TryAgainLevel()
        {
            if (_progressHandler.CurrentLevelNumber == -1)
            {
                Debug.LogWarning($"Current Level is {_progressHandler.CurrentLevelNumber}. Cannot continue. Warning in {this}");
                _screenHandler.ShowChooseLevelScreen();
            }
            
            // _progressHandler.ResetLevelProgress(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber);
            // var levelParams = _progressHandler.GetLevelByNumber(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber);
            
            _screenHandler.ReplayCurrentLevel(_progressHandler.CurrentLevelNumber);
            TryInvokeFinishLevelSessionAction();
            // _screenHandler.PopupAllScreenHandlers();
            // _levelSessionHandler.StartLevel(levelParams, _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
        }
        
        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _replayButton.onClick.RemoveAllListeners();
            
            TryInvokeFinishLevelSessionAction();
        }

        private void TryInvokeFinishLevelSessionAction()
        {
            _onFinishLevelSessionAction?.Invoke();
            _onFinishLevelSessionAction = null;
        }
    }
}