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
using Zenject;

namespace Services
{
    public class AdsService : DisposableService
    {
        [Inject] private readonly BridgeService _bridgeService;
        
        private const float AdTimeoutDuration = 4f; 
        private const float CheatTimeoutDuration = 2f;

        private readonly SoundHandler _soundHandler;
        private readonly PersistentCoroutinesService _coroutinesService;

        private bool _isAdShowing;
        private bool _isResolved;
        private bool _hasAdStarted;
        private IPromise _timeoutPromise;
        private Promise<bool> _currentAdPromise;
        private float _lastInterstitialTime = -1000f;

        public bool CanShowPrerollAd()
        {
            return !_bridgeService.CheckPlatform(BridgePlatformType.Yandex) && !_bridgeService.CheckPlatform(BridgePlatformType.PlayGama);
        }

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
        
        public bool IsInterstitialCooldownPassed()
        {
            if (Bridge.advertisement == null)
                return false;
            
            var delay = Bridge.advertisement.minimumDelayBetweenInterstitial;
            if (delay <= 0) delay = 61; 

            return (Time.time - _lastInterstitialTime) >= delay;
        }

        public IPromise<bool> ShowInterstitial(string placement = "default")
        {
            if (Bridge.advertisement == null)
            {
                LoggerService.LogWarning(this, $"Bridge advertisement is null");
                return Promise<bool>.Resolved(false);
            }
            
            if (!AppConfigService.IsProduction())
            {
                LoggerService.LogDebug($"<color=green>{GetType().Name}</color> Dev Mode: Skipping Interstitial ({placement})");
                return Promise<bool>.Resolved(true);
            }
            
            if (!IsInterstitialCooldownPassed())
            {
                LoggerService.LogDebug(this, "Interstitial skipped: Cooldown is active.");
                return Promise<bool>.Resolved(false);
            }
            
            if (Bridge.advertisement.interstitialState != InterstitialState.Closed)
            {
                LoggerService.LogDebug(this, "Interstitial skipped: Module is busy.");
                return Promise<bool>.Resolved(false);
            }

            return InternalShowAd(() => Bridge.advertisement.ShowInterstitial());
        }

        public IPromise<bool> ShowRewardedAd(string placement = "default")
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
            
            var localAdPromise = new Promise<bool>();
            _currentAdPromise = localAdPromise;
            _timeoutPromise?.SafeCancel();
            _isResolved = false;
            _hasAdStarted = false; 
            _isAdShowing = true;

            SetPause(true);
            
            _timeoutPromise = _coroutinesService.WaitForRealtime(AdTimeoutDuration).Then(() => {
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
                LoggerService.LogWarning($"{GetType().Name} SDK Exception: {e.Message}");
                
                var failPromise = _currentAdPromise; 
                FinalizeAd(false);
                return failPromise;
            }

            return localAdPromise;
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
                    _lastInterstitialTime = Time.time;
                    FinalizeAd(true);
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
            
            _timeoutPromise?.SafeCancel();
            _currentAdPromise?.SafeResolve(result);
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
            if (Bridge.advertisement != null)
            {
                Bridge.advertisement.interstitialStateChanged -= OnInterstitialStateChanged;
                Bridge.advertisement.rewardedStateChanged -= OnRewardedStateChanged;
            }
            
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