using System;

namespace Utilities.StateMachine
{
    public interface IStateFactory
    { 
        IState CreateState(Type type);
        IStateFollower CreateFollower(Type type);
    }
}