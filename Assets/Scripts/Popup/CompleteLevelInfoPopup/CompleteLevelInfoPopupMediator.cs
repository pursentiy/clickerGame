using System;
using System.Collections;
using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Services;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using Random = UnityEngine.Random;

namespace Popup.CompleteLevelInfoPopup
{
    [AssetKey("UI Popups/CompleteLevelInfoPopupMediator")]
    public class CompleteLevelInfoPopupMediator : UIPopupBase<CompleteLevelInfoPopupView, CompleteLevelInfoPopupContext>
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private PlayerCurrencyService _playerCurrencyService;

        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Action _onFinishLevelSessionAction;
        private Coroutine _particlesCoroutine;

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            SetLevelTimeText(Context.TotalTime);
            SetupStars(Context.TotalStars);
            AcquireEarnedStars(Context.EarnedStars);
            
            View.BackgronudButton.onClick.MapListenerWithSound(GoToLevelsMenuScreen).DisposeWith(this);
            View.CloseButton.onClick.MapListenerWithSound(GoToLevelsMenuScreen).DisposeWith(this);
            View.RestartLevelButton.onClick.MapListenerWithSound(TryAgainLevel).DisposeWith(this);
            View.NextLevelButton.onClick.MapListenerWithSound(TryStartNextLevel).DisposeWith(this);
        }

        public void SetOnFinishLevelSessionAction(Action action)
        {
            _onFinishLevelSessionAction = action;
        }

        private void SetLevelTimeText(float time)
        {
            //TODO LOCALIZATION
            View.TimeText.text = $"Time: {time}";
        }

        private void SetupStars(int totalStarsForLevel)
        {
            foreach (var star in View.Stars)
            {
                star.TrySetActive(false);
            }

            for (var i = 0; i < View.Stars.Length; i++)
            {
                if (i >= totalStarsForLevel)
                    View.Stars[i].material = View.GrayScaleMaterial;
                
                View.Stars[i].TrySetActive(true);
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
                View.FireworksParticles[fireworkPosition].Play();
            }
            
            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
            PlayFireworksParticles();
        }

        private void TryAgainLevel()
        {
            if (_playerProgressService.CurrentLevelNumber == -1)
            {
                LoggerService.LogWarning($"Current Level is {_playerProgressService.CurrentLevelNumber}. Cannot continue. Warning in {this}");
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
            var fireworksParticlesArray = new int[View.FireworksParticles.Length];
            for (var i = 0; i < View.FireworksParticles.Length; i++)
            {
                fireworksParticlesArray[i] = i;
            }

            _particlesCoroutine = StartCoroutine(PlayParticles(CollectionExtensions.ShuffleCopy(fireworksParticlesArray)));
        }
    }
}