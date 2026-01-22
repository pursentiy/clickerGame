using System;
using Extensions;
using Handlers;
using Playgama;
using Playgama.Modules.Advertisement;
using RSG;
using Services.Base;
using Services.CoroutineServices;
using Services.Static;
using UnityEngine;
using Utilities.Disposable;

namespace Services
{
    public class AdsService : DisposableService
    {
        private const float AdTimeoutDuration = 10f; 
        private const float CheatTimeoutDuration = 2f;

        private readonly SoundHandler _soundHandler;
        private readonly PersistentCoroutinesService _coroutinesService;

        private bool _isAdShowing;
        private bool _isResolved;
        private bool _hasAdStarted;
        private Promise<bool> _currentAdPromise;

        public AdsService(
            SoundHandler soundHandler, 
            PersistentCoroutinesService coroutinesService)
        {
            _soundHandler = soundHandler;
            _coroutinesService = coroutinesService;
            
            Bridge.advertisement.interstitialStateChanged += OnInterstitialStateChanged;
            Bridge.advertisement.rewardedStateChanged += OnRewardedStateChanged;
        }
        
        public void ShowBanner()
        {
            if (!AppConfigService.IsProduction())
            {
                LoggerService.LogDebug($"<color=green>{GetType().Name}</color> Dev Mode: Skipping Banner");
                return;
            }
            
            if (Bridge.advertisement.isBannerSupported)
            {
                Bridge.advertisement.ShowBanner(BannerPosition.Top);
            }
        }

        public IPromise<bool> ShowInterstitial(string placement = "default")
        {
            if (!AppConfigService.IsProduction())
            {
                LoggerService.LogDebug($"<color=green>{GetType().Name}</color> Dev Mode: Skipping Interstitial ({placement})");
                return Promise<bool>.Resolved(true);
            }

            return InternalShowAd(() => Bridge.advertisement.ShowInterstitial());
        }

        public IPromise<bool> ShowRewarded(string placement = "default")
        {
            if (!AppConfigService.IsProduction())
            {
                LoggerService.LogDebug($"<color=green>{GetType().Name}</color> Dev Mode: Skipping Rewarded ({placement})");
                return Promise<bool>.Resolved(true);
            }

            return InternalShowAd(() => Bridge.advertisement.ShowRewarded());
        }

        private IPromise<bool> InternalShowAd(Action bridgeAction)
        {
            if (_isAdShowing) 
                return Promise<bool>.Resolved(false);
            
            _currentAdPromise = new Promise<bool>();
            _isResolved = false;
            _hasAdStarted = false; 
            _isAdShowing = true;

            SetPause(true);
            
            _coroutinesService.WaitForRealtime(AdTimeoutDuration).Then(() => {
                if (!_isResolved && !_hasAdStarted)
                {
                    LoggerService.LogWarning($"<color=orange>{GetType().Name}</color> Ad Timeout Reached during loading.");
                    FinalizeAd(false);
                }
            }).CancelWith(this);

            try
            {
                bridgeAction.Invoke();
            }
            catch (Exception e)
            {
                LoggerService.LogError($"{GetType().Name} SDK Exception: {e.Message}");
                FinalizeAd(false);
            }

            return _currentAdPromise;
        }

        private void OnInterstitialStateChanged(InterstitialState state)
        {
            if (!_isAdShowing) 
                return;
            
            LoggerService.LogDebug($"<color=cyan>{GetType().Name}</color> Interstitial State: {state}");

            switch (state)
            {
                case InterstitialState.Opened:
                    _hasAdStarted = true;
                    break;
                case InterstitialState.Closed:
                case InterstitialState.Failed:
                    FinalizeAd(true); // В Interstitial всегда true, чтобы не ломать поток игры
                    break;
            }
        }

        private void OnRewardedStateChanged(RewardedState state)
        {
            if (!_isAdShowing) 
                return;
            
            LoggerService.LogDebug($"<color=cyan>{GetType().Name}</color> Rewarded State: {state}");

            switch (state)
            {
                case RewardedState.Opened:
                    _hasAdStarted = true;
                    break;
                case RewardedState.Rewarded:
                    FinalizeAd(true); // Успешный просмотр
                    break;
                case RewardedState.Closed:
                case RewardedState.Failed:
                    FinalizeAd(false); // Награда не получена
                    break;
            }
        }

        private void FinalizeAd(bool result)
        {
            if (_isResolved) 
                return;
            
            _isResolved = true;
            _isAdShowing = false;

            SetPause(false);
            
            var promiseToResolve = _currentAdPromise;
            _currentAdPromise = null;
            promiseToResolve?.SafeResolve(result);
        }

        private void SetPause(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
            AudioListener.pause = isPaused;
            if (isPaused) _soundHandler.MuteAll(); else _soundHandler.UnmuteAll();
        }

        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            FinalizeAd(false);
        }
        
#if UNITY_EDITOR
        public IPromise<bool> CheatShowSuccess()
        {
            return InternalShowAd(() => {
                _coroutinesService.WaitForRealtime(0.5f).Then(() => {
                    OnRewardedStateChanged(RewardedState.Opened);
                    _coroutinesService.WaitForRealtime(0.5f).Then(() => OnRewardedStateChanged(RewardedState.Rewarded));
                });
            });
        }

        public IPromise<bool> CheatShowException()
        {
            return InternalShowAd(() => throw new Exception($"Fake Bridge Crash"));
        }

        public IPromise<bool> CheatShowTimeout()
        {
            // Специально не вызываем bridgeAction, чтобы сработал InternalShowAd таймаут
            // Но для теста через 2 сек, просто имитируем Finalize
            var promise = new Promise<bool>();
            _isAdShowing = true;
            _isResolved = false;
            _hasAdStarted = false;
            _currentAdPromise = promise;
            SetPause(true);

            _coroutinesService.WaitForRealtime(CheatTimeoutDuration).Then(() => FinalizeAd(false));
            return promise;
        }
#endif
    }
}