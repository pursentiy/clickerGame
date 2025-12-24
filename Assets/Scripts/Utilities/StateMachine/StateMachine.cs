using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Extensions;
using Platform.Common.Utilities.StateMachine;
using Services;
using UnityEngine;

namespace Utilities.StateMachine
{
    public sealed class StateMachine : IStateMachine, IStateFactory
    {
        public bool Busy => !_sequences.IsNullOrEmpty() && !_sequencesToAdd.IsNullOrEmpty();
        
        private static readonly Dictionary<Type, Func<IState>> StatesMap = new();
        private static readonly Dictionary<Type, Func<IStateFollower>> FollowersMap = new();

        private readonly List<StateSequence> _sequences = new();
        private readonly List<StateSequence> _sequencesToAdd = new();

        private readonly IStateMachineLog _systemLog;
        private readonly IStateMachineLog _log;

        private readonly IStateContext _context;

        private bool _started;

        public static IStateMachine CreateMachine(IStateContext stateContext)
        {
            return new StateMachine(stateContext);
        }
        
        public StateMachine(IStateContext context, IStateMachineLog systemLog, IStateMachineLog log)
        {
            _context = context;
            _systemLog = systemLog;
            _log = log;
        }

        public StateMachine(IStateContext context)
        {
            _context = context;
        }

        public static void RegisterSpecificStateFactory(Type type, Func<IState> stateFactory)
        {
            StatesMap[type] = stateFactory;
        }
        
        public static void RegisterSpecificStateFollowerFactory(Type type, Func<IStateFollower> followerFactory)
        {
            FollowersMap[type] = followerFactory;
        }

        public IStateSequence StartSequence<TClass>(params object[] arguments) where TClass : IState
        {
            return StartSequence(new StateStartParams(typeof(TClass), null, arguments));
        }

        public IStateSequence StartSequence<TClass, TFollower>(params object[] arguments) where TClass : IState where TFollower : IStateFollower
        {
            return StartSequence(new StateStartParams(typeof(TClass), typeof(TFollower), arguments));
        }

        public IStateSequence StartSequence(Type type, params object[] arguments)
        {
            return StartSequence(new StateStartParams(type, null, arguments));
        }

        public IStateSequence StartSequence(StateStartParams startParams)
        {
            _started = true;
            
            _systemLog?.Debug("Start sequence with " + startParams.State);
            
            var sequence = new StateSequence(this, this, _context, _systemLog, _log);
            sequence.ActivateState(startParams);
            
            _sequencesToAdd.Add(sequence);

            return sequence;
        }

        public void Execute()
        {
            var anyFinished = false;
            
            foreach (var sequence in _sequences)
            {
                if (sequence.CurrentState == StateSequence.State.Running)
                {
                    sequence.OnExecute();
                }
                else
                {
                    anyFinished = true;
                }
            }

            if (_sequencesToAdd.Count > 0)
            {
                foreach (var sequence in _sequencesToAdd)
                {
                    _sequences.Add(sequence);
                }

                _sequencesToAdd.Clear();
            }

            if (anyFinished)
            {
                var removed = _sequences.RemoveAll(x => x.CurrentState == StateSequence.State.Finished);
                _systemLog?.Debug("Removed " + removed  + " sequences");
            }
        }

        public IState CreateState(Type type)
        {
            if (StatesMap.TryGetValue(type, out var func))
            {
                return func();
            }

            return StateFactory.CreateInstance(type) as IState;
        }
        
        public IStateFollower CreateFollower(Type type)
        {
            if (FollowersMap.TryGetValue(type, out var func))
            {
                return func();
            }

            return StateFactory.CreateInstance(type) as IStateFollower;
        }

        public bool Finished
        {
            get
            {
                if (_started && _sequences.IsNullOrEmpty() && _sequencesToAdd.IsNullOrEmpty())
                {
                    return true;
                }
                
                return false;
            }
        }

        public void Release()
        {
            if (Finished)
            {
                return;
            }

            foreach (var sequence in _sequences)
            {
                sequence.Finish();
            }
            
            _sequences.Clear();
            
            foreach (var sequence in _sequencesToAdd)
            {
                sequence.Finish();
            }
            
            _sequencesToAdd.Clear();
        }
    }
    
    public static class StateFactory
    {
        private static readonly Dictionary<Type, Func<object>> ConstructorCache = new();

        public static bool StateMachineFactoryUseExpressionNew = true;
        
        public static object CreateInstance(Type type)
        {
            if (StateMachineFactoryUseExpressionNew)
            {
                try
                {
                    if (!ConstructorCache.TryGetValue(type, out var ctor))
                    {
                        var newExpr = Expression.New(type);
                        var lambda = Expression.Lambda<Func<object>>(newExpr);
                        ctor = lambda.Compile();

                        ConstructorCache[type] = ctor;
                    }

                    return ctor();
                }
                catch (Exception e)
                {
                    LoggerService.LogError($"Error in Expression.New: {e}");
                    StateMachineFactoryUseExpressionNew = false;
                }
            }

            return Activator.CreateInstance(type);
        }
    }
}