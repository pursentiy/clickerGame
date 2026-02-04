using System;
using Common.Currency;
using Common.Data.Info;
using Controllers;
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

namespace UI.Screens.PuzzleAssembly.Level.FinishLevelSequence
{
    public class SaveProgressState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly FlowScreenController _flowScreenController;

        
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
            var packInfo = _progressProvider.GetPackInfo(Context.PackId);
            if (packInfo == null)
            {
                LoggerService.LogWarning(this, $"{nameof(PackInfo)} is null for PackId {Context.PackId}. Returning to Welcome Screen");
                _flowScreenController.GoToWelcomeScreen();
                return;
            }
            
            _flowScreenController.GoToChooseLevelScreen(packInfo);
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