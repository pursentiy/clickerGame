using System;
using JetBrains.Annotations;
using Utilities.Disposable;

namespace Utilities
{
    [UsedImplicitly]
    public sealed class GlobalDisposableService : IDisposable
    {
        public CancelToken Disposables { get; } = new CancelToken();
        public bool IsDisposed { get; private set; }

        void IDisposable.Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            Disposables.Cancel();
        }
    }
}