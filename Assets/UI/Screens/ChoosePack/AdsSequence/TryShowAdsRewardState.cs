using System;
using Common.Currency;
using Handlers;
using RSG;
using Services;
using Services.Player;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.AdsSequence
{
    public class TryShowAdsRewardState : InjectableStateBase<RewardedAdsSequenceContext>
    {
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIBlockHandler _uiBlockHandler;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly GameConfigurationProvider _gameConfigurationProvider;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareForState();
            
            ShowRewardedAds()
                .Then(EvaluateResult)
                .Then(earnedStars =>
                {
                    ResetState();
                    NextState(earnedStars);
                })
                .Catch(e =>
                {
                    LoggerService.LogWarning(this, $"[{nameof(Exception)}]: {e.Message}");
                    ResetState();
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

            var starsToEarn = _gameConfigurationProvider.StarsRewardForAds;
            if (!_playerCurrencyService.TryAddStars(_gameConfigurationProvider.StarsRewardForAds, CurrencyChangeMode.Animated))
            {
                return Promise<Stars>.Resolved(0);
            }
            
            return Promise<Stars>.Resolved(starsToEarn);
        }

        private void NextState(Stars stars)
        {
            Sequence.ActivateState<VisualizeAdsRewardsState>(new RewardsEarnedInfo(_playerCurrencyService.Stars, stars));
        }

        private void PrepareForState()
        {
            _uiBlockHandler.BlockUserInput(true);
        }

        private void ResetState()
        {
            _uiBlockHandler.BlockUserInput(false);
        }
    }
}