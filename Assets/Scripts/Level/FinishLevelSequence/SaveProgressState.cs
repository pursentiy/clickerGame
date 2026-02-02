using System;
using Common.Currency;
using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.Player;
using Services.ScreenBlocker;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Level.FinishLevelSequence
{
    public class SaveProgressState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        
        private IUIBlockRef _uiBlockRef;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            PrepareEnvironment();
            SaveProgress()
                .ContinueWithResolved(GoToLevelsScreen)
                .CancelWith(this);
        }

        private IPromise SaveProgress()
        {
            var starsEarned = Math.Max(0, Context.CurrentStars - Context.InitialStars);
            SetLevelCompleted(Context.PackId, Context.LevelId, Context.LevelCompletingTime, Context.CurrentStars);
            AcquireEarnedStars(starsEarned);

            return _coroutineService.WaitFrame();
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

        private void GoToLevelsScreen()
        {
            if (Context.PackInfo == null)
            {
                LoggerService.LogWarning(this, $"[{GetType().Name}]: {nameof(Context.PackInfo)} is null. Returning to Welcome Screen");
                _screenHandler.ShowWelcomeScreen();
                return;
            }
            
            //_screenHandler.ShowChooseLevelScreen(Context.PackInfo);
            RevertEnvironment();
            Sequence.Finish();
        }
        
        private void PrepareEnvironment()
        {
            _uiBlockRef = _uiScreenBlocker.Block();
        }

        private void RevertEnvironment()
        {
            _uiBlockRef?.Dispose();
        }
    }
}