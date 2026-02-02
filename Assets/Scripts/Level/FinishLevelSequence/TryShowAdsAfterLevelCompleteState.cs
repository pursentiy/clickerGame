using Extensions;
using JetBrains.Annotations;
using RSG;
using Services;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Level.FinishLevelSequence
{
    [UsedImplicitly]
    public sealed class TryShowAdsAfterLevelCompleteState : InjectableStateBase<FinishLevelContext>
    {
        private const float ScreenBlockTime = 300;
        
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;

        private IUIBlockRef _uiBlockRef;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareForState();
            
            ShowInterstitialAds()
                .ContinueWithResolved(() =>
                {
                    ResetState();
                    NextState();
                })
                .CancelWith(this);
        }

        private IPromise ShowInterstitialAds()
        {
            return _adsService.ShowInterstitial().AsNonGenericPromise().CancelWith(this);
        }

        private void NextState()
        {
            Sequence.ActivateState<SaveProgressState>();
        }

        private void PrepareForState()
        {
            _uiBlockRef = _uiScreenBlocker.Block(ScreenBlockTime);
        }

        private void ResetState()
        {
            _uiBlockRef?.Dispose();
        }
    }
}