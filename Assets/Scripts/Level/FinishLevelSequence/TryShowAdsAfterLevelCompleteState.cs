using Extensions;
using Handlers;
using JetBrains.Annotations;
using RSG;
using Services;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Level.FinishLevelSequence
{
    [UsedImplicitly]
    public sealed class TryShowAdsAfterLevelCompleteState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly AdsService _adsService;
        [Inject] private readonly UIBlockHandler _uiBlockHandler;

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
            _uiBlockHandler.BlockUserInput(true);
        }

        private void ResetState()
        {
            _uiBlockHandler.BlockUserInput(false);
        }
    }
}