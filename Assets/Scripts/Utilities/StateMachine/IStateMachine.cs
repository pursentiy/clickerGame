using System;

namespace Platform.Common.Utilities.StateMachine
{
    public interface IStateMachine
    {
        IStateSequence StartSequence<TClass>(params object[] arguments) where TClass : IState;
        IStateSequence StartSequence<TClass, TFollower>(params object[] arguments) where TClass : IState where TFollower : IStateFollower;
        IStateSequence StartSequence(Type type, params object[] arguments);
        bool Finished { get; }
        bool Busy { get; }
        void Execute();
        void Release();
    }
}