using System;
using Extensions;
using Platform.Common.Utilities.StateMachine;
using UnityEngine;
using Utilities.Disposable;

namespace Utilities.StateMachine
{
    public abstract class StateBase<TContext> : IState, IDisposable, IDisposeProvider where TContext : class, IStateContext
    {
        protected TContext Context { get; private set; }
        
        protected IStateMachine Machine { get; private set; }
        
        protected IStateSequence Sequence { get; private set; }

        protected IStateMachineLog Log { get; private set; }
        
        protected object[] Arguments { get; private set; }
        
        /// <summary>
        /// Should not be used in normal situations. 
        /// Unsubscribe actions with DisposeWith, CancelWith etc instead. 
        /// </summary>
        protected bool Exited { get; private set; }
        
        public void Setup(IStateContext context, IStateMachine machine, IStateSequence sequence, IStateMachineLog log)
        {
            Machine = machine;
            Context = context as TContext;
            Sequence = sequence;
            Log = log;
        }

        public void SetupArguments(object[] arguments)
        {
            Arguments = arguments;
        }

        public virtual void OnEnter(params object[] arguments)
        {

        }

        public virtual void OnExecute()
        {
            
        }

        public virtual void OnExit()
        {
            Exited = true;
            DisposeService.HandledDispose(this);
        }

        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        public void Dispose()
        {
            DisposeService.HandledDispose(this);
        }
        
        #region Arguments
        
        protected T GetArgument<T>(int index) where T : class
        {
            return GetArgument<T>(index, default);
        }

        protected T GetArgument<T>(int index, T defaultValue) where T : class
        {
            if (!IsArgumentIndexValid(index))
            {
                return defaultValue;
            }

            return Arguments[index] as T;
        }

        protected bool GetBoolArgument(int index, bool defaultValue = false)
        {
            if (!IsArgumentIndexValid(index))
            {
                return defaultValue;
            }

            if (Arguments[index] is bool value)
            {
                return value;
            }

            return defaultValue;
        }

        private bool IsArgumentIndexValid(int index)
        {
            if (Arguments.IsNullOrEmpty())
            {
                return false;
            }

            if (index < 0 || index >= Arguments.Length)
            {
                return false;
            }

            return true;
        }
        
        #endregion
    }
}