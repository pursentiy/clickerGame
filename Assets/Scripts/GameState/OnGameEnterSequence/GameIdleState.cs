using JetBrains.Annotations;
using Platform.Common.Utilities.StateMachine;
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