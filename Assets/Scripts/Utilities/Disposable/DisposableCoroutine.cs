using System;
using System.Collections;
using Services;
using Services.CoroutineServices;
using UnityEngine;

namespace Utilities.Disposable
{
    public class DisposableCoroutine : IDisposable, IDisposeProvider
    {
        private readonly CoroutineService _coroutineService;
        private Coroutine _coroutine;
        
        public DisposableCoroutine(CoroutineService coroutineService, IEnumerator routine)
        {
            _coroutineService = coroutineService;
            _coroutine = _coroutineService.StartCoroutine(ExecutionLoop(routine));
        }

        private IEnumerator ExecutionLoop(IEnumerator routine)
        {
            yield return routine;
            Dispose();
        }
        
        public void Dispose()
        {
            if (_coroutine != null)
            {
                _coroutineService.StopCoroutine(_coroutine);
            }
            
            _coroutine = null;
            
            DisposeService.HandledDispose(this);
        }

        public DisposableCollection ChildDisposables => _collection ?? (_collection = new DisposableCollection());
        private DisposableCollection _collection;
    }   
}
