using Services;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Level.FinishLevelSequence
{
    public class ResetLevelParamsState : InjectableStateBase<FinishLevelContext>
    {
        [Inject] private readonly ProgressController _progressController;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            _progressController.ResetCurrentLevelSnapshot();
            NextState();
        }

        private void NextState()
        {
            Sequence.ActivateState<ShowCompletePopupState>();
        }
    }
}