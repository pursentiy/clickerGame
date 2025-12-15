using System;
using System.Collections.Generic;
using Platform.Common.Utilities.StateMachine;

namespace Utilities.StateMachine
{
    public sealed class StateSequence : IStateSequence
    {
        public State CurrentState { get; private set; }
        public bool IsActive => CurrentState == State.Running;

        private List<Action> _finishActions;
        private readonly IStateMachineLog _systemLog;
        private readonly IStateMachineLog _log;
        
        private IState _currentState;
        private IStateFollower _currentStateFollower;

        private readonly IStateMachine _machine;
        private readonly IStateFactory _factory;
        private readonly IStateContext _context;
        
        public StateSequence(
            IStateFactory factory,
            IStateMachine machine,
            IStateContext context,
            IStateMachineLog systemLog,
            IStateMachineLog log)
        {
            _machine = machine;
            _factory = factory;
            _context = context;
            _systemLog = systemLog;
            _log = log;
        }

        public void ActivateState<TClass>(params object[] arguments) where TClass : IState
        {
            ActivateState(new StateStartParams(typeof(TClass), null, arguments));
        }
        
        public void ActivateState<TClass, TFollower>(params object[] arguments) where TClass : IState where TFollower : IStateFollower
        {
            ActivateState(new StateStartParams(typeof(TClass), typeof(TFollower), arguments));
        }

        public void ActivateState(Type type, params object[] arguments)
        {
            ActivateState(new StateStartParams(type, null, arguments));
        }
        
        public bool CompareCurrentState<TClass>(params object[] arguments) where TClass : IState
        {
            return _currentState.GetType() == typeof(TClass);
        }

        public void ActivateState(StateStartParams startParams)
        {
            if (CurrentState != State.Running)
            {
                return;
            }

            _currentStateFollower?.OnExit();
            _currentStateFollower = null;

            if (_currentState != null)
            {
                _currentState.OnExit();
                _currentState = null;
            }

            var targetState = _factory.CreateState(startParams.State);
            _currentState = targetState;

            if (startParams.Follower != null)
            {
                _currentStateFollower = _factory.CreateFollower(startParams.Follower);
                _currentStateFollower?.OnEnter();
            }
            
            _currentState.Setup(_context, _machine,this, _log);
            
            _currentState.SetupArguments(startParams.Arguments);
            _currentState.OnEnter(startParams.Arguments);
        }

        public void OnExecute()
        {
            _currentState?.OnExecute();
        }

        public void Finish()
        {
            if (CurrentState != State.Running)
            {
                return;
            }
            
            CurrentState = State.Finished;
            
            _currentStateFollower?.OnExit();
            _currentState?.OnExit();

            _currentStateFollower = null;
            _currentState = null;

            if (_finishActions == null)
            {
                return;
            }

            foreach (var finish in _finishActions)
            {
                finish?.Invoke();
            }
        }

        public IStateSequence OnFinish(Action callback)
        {
            if (CurrentState != State.Running)
            {
                callback?.Invoke();
                return this;
            }

            if (_finishActions == null)
            {
                _finishActions = new List<Action>();
            }
            
            _finishActions.Add(callback);
            return this;
        }

        public void Cancel()
        {
            if (CurrentState != State.Running)
            {
                return;
            }
            
            CurrentState = State.Canceled;
            
            _currentStateFollower?.OnExit();
            _currentState?.OnExit();
            
            _currentStateFollower = null;
            _currentState = null;

            if (_finishActions == null)
            {
                return;
            }

            foreach (var finish in _finishActions)
            {
                finish?.Invoke();
            }
        }

        public enum State
        {
            Running,
            Finished,
            Canceled
        }
    }
}