using System;

namespace Utilities.StateMachine
{
    public readonly struct StateStartParams
    {
        public StateStartParams(Type state, Type follower, object[] arguments)
        {
            State = state;
            Follower = follower;
            Arguments = arguments;
        }

        public Type State { get; }
        public Type Follower { get; }
        public object[] Arguments { get; }
    }
}