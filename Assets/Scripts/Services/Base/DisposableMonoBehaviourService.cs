using Extensions;
using UnityEngine;
using Utilities.Disposable;

namespace Services.Base
{
    public abstract class DisposableMonoBehaviourService : MonoBehaviour, IDisposableService
    {
        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        private bool _disposed;

        public void InitializeService()
        {
            _disposed = false;
            LoggerService.LogDebug(this, "Initialized");
            OnInitialize();
        }

        protected abstract void OnInitialize();

        public void DisposeService()
        {
            if (_disposed)
            {
                LoggerService.LogWarning($"A call to dispose service {GetType().Name} which has already been disposed");
                return;
            }
            
            LoggerService.LogDebug(this, "Started disposing");
            OnDisposing();
            DisposableExtensions.HandledDispose(this);
            LoggerService.LogDebug(this, "Disposed");
            _disposed = true;
        }

        protected abstract void OnDisposing();
    }
}