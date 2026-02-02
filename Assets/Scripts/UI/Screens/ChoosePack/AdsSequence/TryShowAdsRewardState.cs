using System;
using Common.Currency;
using RSG;
using Services;
using Services.Configuration;
using Services.Player;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.AdsSequence
{
    public class TryShowAdsRewardState : InjectableStateBase<RewardedAdsSequenceContext>
    {
        private const float ScreenBlockTime = 300;
        
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly GameInfoProvider _gameInfoProvider;
        
        private IUIBlockRef _uiBlockRef;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareEnvironment();
            
            ShowRewardedAds()
                .Then(EvaluateResult)
                .Then(NextState)
                .Catch(e =>
                {
                    LoggerService.LogWarning(this, $"[{nameof(Exception)}]: {e.Message}");
                    NextState(0);
                })
                .CancelWith(this);
            
        }

        private IPromise<bool> ShowRewardedAds()
        {
            return _adsService.ShowRewardedAd().CancelWith(this);
        }

        private IPromise<Stars> EvaluateResult(bool result)
        {
            if (!result)
                return Promise<Stars>.Resolved(0);

            var starsToEarn = _gameInfoProvider.StarsRewardForAds;
            if (!_playerCurrencyService.TryAddStars(_gameInfoProvider.StarsRewardForAds, CurrencyChangeMode.Animated))
            {
                return Promise<Stars>.Resolved(0);
            }
            
            return Promise<Stars>.Resolved(starsToEarn);
        }

        private void NextState(Stars stars)
        {
            RevertEnvironment();
            Sequence.ActivateState<VisualizeAdsRewardsState>(new RewardsEarnedInfo(_playerCurrencyService.Stars, stars));
        }

        private void PrepareEnvironment()
        {
            _uiBlockRef = _uiScreenBlocker.Block(ScreenBlockTime);
        }

        private void RevertEnvironment()
        {
            _uiBlockRef?.Dispose();
        }
    }
}