using System;
using System.Collections.Generic;
using System.Linq;
using Services;
using UnityEngine;

namespace Utilities.Disposable
{
    public sealed class DisposableCollection : IDisposable
    {
        private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();       
        
        public void Add(IDisposable disposable)
        {
            if (disposable != null && !_disposables.Contains(disposable))
            {
                _disposables.Add(disposable);
            }
        }

        public bool Remove(IDisposable disposable)
        {
            return _disposables.Remove(disposable);
        }

        public void Dispose()
        {
            _disposables.ToList().ForEach(DisposeAction);
            _disposables.Clear();
        }

        private void DisposeAction(IDisposable x)
        {
            try
            {
                x.Dispose();
            }
            catch (Exception e)
            {
                var type = x != null ? x.GetType().Name : string.Empty;
                LoggerService.LogError($"Error when disposing {type}: {e}");
            }
        }
    }
}