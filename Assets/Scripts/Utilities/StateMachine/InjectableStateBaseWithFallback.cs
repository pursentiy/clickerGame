using System;
using Installers;
using Platform.Common.Utilities.StateMachine;
using UnityEngine;

namespace Utilities.StateMachine
{
    /// <summary>
    /// Falls to TFallbackState state in case of exception in OnEnter
    /// TFallbackState must accept exception in Arguments[0]
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TFallbackState"></typeparam>
    public abstract class InjectableStateBaseWithFallback<TContext, TFallbackState> : StateBase<TContext> where TContext : class, IStateContext
        where TFallbackState : IState
    {
        public sealed override void OnEnter(params object[] arguments)
        {
            ContainerHolder.CurrentContainer.Inject(this);
            
            base.OnEnter(arguments);

            try
            {
                OnSafeEnter(arguments);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Sequence.ActivateState<TFallbackState>(e);
            }
        }

        protected virtual void OnSafeEnter(params object[] arguments)
        {
            
        }

        public sealed override void OnExit()
        {
            base.OnExit();
        }
    }
}