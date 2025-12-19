using System;
using System.Linq;
using UnityEngine;

namespace Plugins.FSignal
{
    public class FPromisedSignal<T> : IDisposable
    {
        public event Action<T> Listener = delegate { };

        private bool _hasResult;
        private T _result;

        private readonly bool _restrictToOnce;
        
        public bool HasResult => _hasResult;
        
        public FPromisedSignal(bool restrictToOnce = false)
        {
            _restrictToOnce = restrictToOnce;
        }

        public void AddListener(Action<T> callback)
        {
            Listener = AddUnique(Listener, callback);
        }

        public void RemoveListener(Action<T> callback) { Listener -= callback; }
        
        public void Dispatch(T type1)
        {
            if (_restrictToOnce && _hasResult)
            {
                Debug.LogError(GetType().Name + " already has result. Resetting it.");
                return;
            }

            _hasResult = true;
            _result = type1;
            
            Listener?.Invoke(type1);
        }

        private Action<T> AddUnique(Action<T> listeners, Action<T> callback)
        {
            if (!listeners.GetInvocationList().Contains(callback))
            {
                if (_hasResult)
                {
                    callback?.Invoke(_result);
                }
                    
                listeners += callback;
            }
            
            return listeners;
        }

        public void Reset()
        {
            ResetResult();
            
            Listener = delegate { };
        }
        
        public void ResetResult()
        {
            _hasResult = false;
            _result = default;
        }

        public void Dispose()
        {
            Reset();
        }
    }

    public class FPromisedSignal : IDisposable
    {
        public event Action Listener = delegate { };

        private bool _hasResult;

        private readonly bool _restrictToOnce;

        public FPromisedSignal(bool restrictToOnce = false)
        {
            _restrictToOnce = restrictToOnce;
        }

        public void AddListener(Action callback)
        {
            Listener = AddUnique(Listener, callback);
        }

        public void RemoveListener(Action callback)
        {
            Listener -= callback;
        }

        public void Dispatch()
        {
            if (_restrictToOnce && _hasResult)
            {
                Debug.LogWarning(GetType().Name + " already has result");
                return;
            }

            _hasResult = true;

            Listener?.Invoke();
        }

        private Action AddUnique(Action listeners, Action callback)
        {
            if (!listeners.GetInvocationList().Contains(callback))
            {
                if (_hasResult)
                {
                    callback?.Invoke();
                }

                listeners += callback;
            }

            return listeners;
        }

        public bool HasResult => _hasResult;

        public void Reset()
        {
            ResetResult();
            Listener = delegate { };
        }

        public void ResetResult()
        {
            _hasResult = false;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}