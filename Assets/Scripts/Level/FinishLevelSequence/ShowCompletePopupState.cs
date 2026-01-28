using System;
using Common.Currency;
using Extensions;
using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.Player;
using UI.Popups.CompleteLevelInfoPopup;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Level.FinishLevelSequence
{
    public class ShowCompletePopupState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly ProgressController _progressController;

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _soundHandler.PlaySound("finished");

            var preRewardsBalance = _playerCurrencyService.Stars;
            var starsEarned = Math.Max(0, Context.CurrentStars - Context.InitialStars);
            SetLevelCompleted(Context.PackId, Context.LevelId, Context.LevelCompletingTime, Context.CurrentStars);
            AcquireEarnedStars(starsEarned);
            
            ShowCompletePopup(Context.CurrentStars, Context.InitialStars, preRewardsBalance, Context.LevelCompletingTime, Context.CompletedLevelStatus)
                .Then(GoToLevelsMenu)
                .ContinueWithResolved(FinishSequence)
                .CancelWith(this);
        }
        
        private void AcquireEarnedStars(Stars earnedStarsForLevel)
        {
            if (Context.CurrentStars <= 0)
                return;
            
            _playerCurrencyService.TryAddStars(earnedStarsForLevel, CurrencyChangeMode.Animated);
        }

        private void SetLevelCompleted(int packId, int levelId, float levelCompletedTime, Stars starsEarned)
        {
            _progressController.SetLevelCompleted(packId, levelId, levelCompletedTime, starsEarned, SavePriority.Default);
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
        
        private void GoToLevelsMenu()
        {
            if (Context.PackInfo == null)
            {
                LoggerService.LogWarning(this, $"[{nameof(ShowCompletePopup)}]: {nameof(Context.PackInfo)} is null. Returning to Welcome Screen");
                _screenHandler.ShowWelcomeScreen();
                return;
            }
            
            _screenHandler.ShowChooseLevelScreen(Context.PackInfo);
        }

        private void FinishSequence()
        {
            Sequence.Finish();
        }
    }
}