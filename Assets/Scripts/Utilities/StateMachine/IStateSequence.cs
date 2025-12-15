using System;
using Utilities.StateMachine;

namespace Platform.Common.Utilities.StateMachine
{
    public interface IStateSequence
    {
        bool IsActive { get; }
        void ActivateState<T>(params object[] arguments) where T : IState;
        void ActivateState<T, TF>(params object[] arguments) where T : IState where TF : IStateFollower;
        void ActivateState(Type type, params object[] arguments);
        void ActivateState(StateStartParams startParams);
        void Finish();
        IStateSequence OnFinish(Action callback);
        StateSequence.State CurrentState { get; }
        void Cancel();
        bool CompareCurrentState<T>(params object[] arguments) where T : IState;
    }
}