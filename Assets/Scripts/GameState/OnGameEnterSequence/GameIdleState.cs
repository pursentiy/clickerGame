using JetBrains.Annotations;
using Utilities.StateMachine;

namespace GameState.OnGameEnterSequence
{
    [UsedImplicitly]
    public class GameIdleState : InjectableStateBase<DefaultStateContext>
    {
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

        }
    }
}