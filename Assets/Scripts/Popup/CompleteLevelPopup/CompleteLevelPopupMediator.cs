using System;
using System.Collections;
using Animations;
using Extensions;
using Handlers;
using Popup.Base;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;
using Random = UnityEngine.Random;

namespace Popup.CompleteLevelPopup
{
    public class CompleteLevelPopupMediator : PopupBase<CompleteLevelPopupContext>
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _restartLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Image[] _stars;
        [SerializeField] private Material _grayScaleMaterial;
        [SerializeField] private RectTransform _screenTransform;
        [SerializeField] private ParticleSystem[] _fireworksParticles;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private ScreenColorAnimation _screenColorAnimation;

        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Action _onFinishLevelSessionAction;
        private Coroutine _particlesCoroutine;

        protected override void OnCreated()
        {
            base.OnCreated();

            SetLevelTimeText(Context.TotalTime);
            SetupStars(Context.TotalStars);
            AcquireEarnedStars(Context.EarnedStars);
            
            _backButton.onClick.MapListenerWithSound(GoToLevelsMenuScreen).DisposeWith(this);
            _restartLevelButton.onClick.MapListenerWithSound(TryAgainLevel).DisposeWith(this);
            _nextLevelButton.onClick.MapListenerWithSound(TryStartNextLevel).DisposeWith(this);
        }

        public void StartColorAnimationLoop(Gradient colorGradient)
        {
            _screenColorAnimation.StartColorLoop(colorGradient);
        }

        public void SetOnFinishLevelSessionAction(Action action)
        {
            _onFinishLevelSessionAction = action;
        }

        private void SetLevelTimeText(float time)
        {
            //TODO LOCALIZATION
            _timeText.text = $"Time: {time}";
        }

        private void SetupStars(int totalStarsForLevel)
        {
            foreach (var star in _stars)
            {
                star.TrySetActive(false);
            }

            for (var i = 0; i < _stars.Length; i++)
            {
                if (i >= totalStarsForLevel)
                    _stars[i].material = _grayScaleMaterial;
                
                _stars[i].TrySetActive(true);
            }
        }

        private void AcquireEarnedStars(int earnedStarsForLevel)
        {
            _playerCurrencyService.AddStars(earnedStarsForLevel);
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

        private void TryAgainLevel()
        {
            if (_playerProgressService.CurrentLevelNumber == -1)
            {
                Debug.LogWarning($"Current Level is {_playerProgressService.CurrentLevelNumber}. Cannot continue. Warning in {this}");
                _screenHandler.ShowChooseLevelScreen();
            }

            _screenHandler.ReplayCurrentLevel(_playerProgressService.CurrentLevelNumber);
            TryInvokeFinishLevelSessionAction();
        }

        private void TryStartNextLevel()
        {
            //TODO NEXT LEVEL LOGIC HERE
        }

        private void GoToLevelsMenuScreen()
        {
            _screenHandler.ShowChooseLevelScreen();
        }
        
        private void OnDestroy()
        {
            TryInvokeFinishLevelSessionAction();
            
            if (_particlesCoroutine != null)
                StopCoroutine(_particlesCoroutine);
        }

        private void TryInvokeFinishLevelSessionAction()
        {
            _onFinishLevelSessionAction?.Invoke();
            _onFinishLevelSessionAction = null;
        }
        
        private void PlayFireworksParticles()
        {
            var fireworksParticlesArray = new int[_fireworksParticles.Length];
            for (var i = 0; i < _fireworksParticles.Length; i++)
            {
                fireworksParticlesArray[i] = i;
            }

            _particlesCoroutine = StartCoroutine(PlayParticles(CollectionExtensions.ShuffleCopy(fireworksParticlesArray)));
        }
    }
}