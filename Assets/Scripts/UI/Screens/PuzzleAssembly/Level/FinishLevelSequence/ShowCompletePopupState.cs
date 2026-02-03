using System;
using Common.Currency;
using Extensions;
using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.Player;
using UI.Popups.CompleteLevelInfoPopup;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Level.FinishLevelSequence
{
    public class ShowCompletePopupState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly CoroutineService _coroutineService;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _soundHandler.PlaySound("finished");

            var preRewardsBalance = _playerCurrencyService.Stars;
            
            _coroutineService.WaitFor(Context.AwaitTimeBeforeShowingPopup)
                .Then(() => ShowCompletePopup(Context.CurrentStars, Context.InitialStars, preRewardsBalance, Context.LevelCompletingTime, Context.CompletedLevelStatus))
                .ContinueWithResolved(NextState)
                .CancelWith(this);
        }

        private IPromise ShowCompletePopup(Stars currentStars, Stars initialStars, Stars preRewardBalance, float beatTime, CompletedLevelStatus levelStatus)
        {
            var popupPromise = new Promise();
            var context = new CompleteLevelInfoPopupContext(currentStars, initialStars, preRewardBalance, beatTime, levelStatus);
            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(context)
                .Then(popup => popup.OnHide(popupPromise.SafeResolve))
                .Catch(OnException)
                .CancelWith(this);

            return popupPromise;

            void OnException(Exception e)
            {
                LoggerService.LogWarning(this, e.Message);
                popupPromise.SafeResolve();
            }
        }

        private void NextState()
        {
            Sequence.ActivateState<TryShowAdsAfterLevelCompleteState>();
        }
    }
}