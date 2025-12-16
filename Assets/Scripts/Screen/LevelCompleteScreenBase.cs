using System;
using System.Collections;
using Animations;
using Extensions;
using Handlers;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Screen
{
    public class LevelCompleteScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressService _progressService;
        [Inject] private SoundHandler _soundHandler;
        
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

            StartCoroutine(PlayParticles(CollectionExtensions.ShuffleCopy(fireworksParticlesArray)));
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
            _backButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _screenHandler.ShowChooseLevelScreen();
            });
            _replayButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                TryAgainLevel();
            });
        }

        private void TryAgainLevel()
        {
            if (_progressService.CurrentLevelNumber == -1)
            {
                Debug.LogWarning($"Current Level is {_progressService.CurrentLevelNumber}. Cannot continue. Warning in {this}");
                _screenHandler.ShowChooseLevelScreen();
            }

            _screenHandler.ReplayCurrentLevel(_progressService.CurrentLevelNumber);
            TryInvokeFinishLevelSessionAction();
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