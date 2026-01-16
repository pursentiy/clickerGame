using Platform.Common.Utilities.StateMachine;

namespace Utilities.StateMachine
{
    public interface IState
    {
        void Setup(IStateContext context, IStateMachine machine, IStateSequence sequence, IStateMachineLog log);
        void SetupArguments(object[] arguments);
        void OnEnter(params object[] arguments);
        void OnExecute();
        void OnExit();
    }

    public interface IStateFollower
    {
        void OnEnter();
        void OnExit();
    }
}