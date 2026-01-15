using Extensions;
using Installers;
using Platform.Common.Utilities.StateMachine;
using Services;
using UnityEngine;

namespace Utilities.StateMachine
{
    public abstract class InjectableStateBase<TContext> : StateBase<TContext> where TContext : class, IStateContext
    {
        public override void OnEnter(params object[] arguments)
        {
            ContainerHolder.CurrentContainer.Inject(this);
            LoggerService.LogDebug($"Enter:{GetType().Name}");
            
            base.OnEnter(arguments);
        }

        public override void OnExit()
        {
            LoggerService.LogDebug($"Exit:{GetType().Name}");
            base.OnExit();
        }
    }

    public abstract class InjectableStateBase<TContext, TArgument> : InjectableStateBase<TContext> where TContext : class, IStateContext
    {
        protected TArgument TypedArgument => Arguments != null && 0.InListRange(Arguments) ? (TArgument)Arguments[0] : default;

        protected bool TryGetArgument(out TArgument argument)
        {
            if (Arguments != null && 0.InListRange(Arguments) && Arguments[0] is TArgument) 
            {
                argument = (TArgument)Arguments[0];
                return true;
            }

            argument = default;
            return false;
        }
    }
    
    public abstract class InjectableStateBase<TContext, TArgument, TArgument1> : InjectableStateBase<TContext> where TContext : class, IStateContext
    {
        protected TArgument TypedArgument => Arguments != null && 0.InListRange(Arguments) ? (TArgument) Arguments[0] : default;
        protected TArgument1 TypedArgument2 => Arguments != null && 1.InListRange(Arguments) ? (TArgument1)Arguments[1] : default;
    }
    
    public abstract class InjectableStateBase<TContext, TArgument, TArgument1, TArgument2> : InjectableStateBase<TContext> where TContext : class, IStateContext
    {
        protected TArgument TypedArgument => Arguments != null && 0.InListRange(Arguments) ? (TArgument)Arguments[0] : default;
        protected TArgument1 TypedArgument2 => Arguments != null && 1.InListRange(Arguments) ? (TArgument1)Arguments[1] : default;
        protected TArgument2 TypedArgument3 => Arguments != null && 2.InListRange(Arguments) ? (TArgument2)Arguments[2] : default;
    }
}