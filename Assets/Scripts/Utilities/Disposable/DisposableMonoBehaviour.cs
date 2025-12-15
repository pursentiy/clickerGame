using System;
using UnityEngine;

namespace Utilities.Disposable
{
    public class DisposableMonoBehaviour : MonoBehaviour, IDisposeProvider, IDisposable
    {
        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        public virtual void Dispose()
        {
            DisposeService.HandledDispose(this);
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}