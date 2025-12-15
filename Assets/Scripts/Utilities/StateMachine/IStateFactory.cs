using System;

namespace Platform.Common.Utilities.StateMachine
{
    public interface IStateFactory
    { 
        IState CreateState(Type type);
        IStateFollower CreateFollower(Type type);
    }
}