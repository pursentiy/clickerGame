using System;

namespace Utilities.Disposable
{
    public abstract class DisposableEntityBase : IDisposeProvider, IDisposable
    {
        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        private bool _disposed;
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            DisposeService.HandledDispose(this);
            _disposed = true;
        }
    }
}