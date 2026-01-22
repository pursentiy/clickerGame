using System;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.CompleteLevelInfoPopup;
using RSG;
using Services;
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

        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _soundHandler.PlaySound("finished");
            
            ShowCompletePopup()
                .Then(GoToLevelsMenu)
                .ContinueWithResolved(FinishSequence)
                .CancelWith(this);
        }

        private IPromise ShowCompletePopup()
        {
            var popupPromise = new Promise();
            var context = new CompleteLevelInfoPopupContext(Context.EarnedStars, Context.EarnedStars, GoToLevelsMenu);
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
                LoggerService.LogError(this, $"[{nameof(ShowCompletePopup)}]: {nameof(Context.PackInfo)} is null. Returning to Welcome Screen");
                _screenHandler.ShowWelcomeScreen();
                return;
            }
            
            _screenHandler.ShowChooseLevelScreen(Context.PackInfo);
        }

        private void FinishSequence()
        {
            //TODO UPDATE STARS PROGRESS IF FAILED
            Sequence.Finish();
        }
    }
}