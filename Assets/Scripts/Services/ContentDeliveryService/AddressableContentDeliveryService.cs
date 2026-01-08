using System;
using Extensions;
using RSG;
using Services.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

namespace Services.ContentDeliveryService
{
    public class AddressableContentDeliveryService : DisposableService
    {
        private const float AddressablesTimeout = 10f;
        private const float SendErrorTimeout = 10f;
        private const float Timeout = 10f;
        
        [Inject] private CoroutineService _coroutineService;
        
        public IPromise<DisposableAssetInstance> InstantiateAsync(
            AssetReference reference,
            Transform parent = null,
            bool inWorldSpace = false)
        {
            LoggerService.LogDebug(this, $"Instantiate addressable asset with reference = {reference}");
            
            var promise = new Promise<DisposableAssetInstance>();
            
            InstantiateAsyncInternal(promise, reference, parent, inWorldSpace);
            
            return promise;
        }
        
        public IPromise<IDisposableContent<T>> LoadAssetAsync<T>(string id, IPromise middleware = null, Action<float> progressCallback = null, float? timeout = null) 
            where T : Object
        {
            LoggerService.LogDebug(this, $"Started loading addressable {id}");

            var promise = new Promise<IDisposableContent<T>>();
            
            LoadAssetAsyncInternal(promise, id, middleware, progressCallback, timeout);
            
            return promise;
        }
        
        
         private void LoadAssetAsyncInternal<T>(Promise<IDisposableContent<T>> promise, string id, 
            IPromise middleware = null, Action<float> progressCallback = null, float? timeout = null) where T : Object
        {
            var timeStarted = Time.realtimeSinceStartup;
            
            Debug.Log($"[DEBUG] Starting {nameof(LoadAssetAsyncInternal)} Addressables Load for ID: {id}");
            var handle = Addressables.LoadAssetAsync<T>(id);

            timeout ??= AddressablesTimeout;
            
            handle.OnResult(middleware ?? Promise.Resolved(), _coroutineService, timeout.Value, SendErrorTimeout, id).Then(either =>
            {
                var passed = Time.realtimeSinceStartup - timeStarted;
                
                either.Match(res =>
                    {
                        if (promise.CurState != PromiseState.Pending)
                        {
                            ReleaseHandle(handle);
                            LoggerService.LogDebug(this, $"Loaded addressable {id} but promise is in state {promise.CurState} [{passed}s]");
                            return;
                        }
                        LoggerService.LogDebug(this, $"Loaded addressable {id} [{passed}s]");
                        promise.Resolve(new DisposableAsset<T>(this, handle, res));
                    }, 
                    exception =>
                    {
                        ReleaseHandle(handle);
                    
                        if (promise.CurState != PromiseState.Pending)
                        {
                            LoggerService.LogDebug(this, $"Failed loading addressable {id} {exception} but promise is in state {promise.CurState} [{passed}s]");
                            return;
                        }
                        LoggerService.LogDebug(this, $"Failed loading addressable {id} {exception} [{passed}s]");
                        promise.Reject(exception);
                    });
            });
        }
        
        public void ReleaseHandle<T>(AsyncOperationHandle<T> handle)
        {
            // Проверяем не только валидность, но и завершенность, если это возможно
            if (handle.IsValid())
            {
                try
                {
                    // В WebGL безопасно релизить только то, что валидно.
                    // Если handle.Result еще в процессе, Release может вызвать сбой.
                    Addressables.Release(handle);
                }
                catch (Exception e)
                {
                    // Используйте лог, который не останавливает выполнение
                    LoggerService.LogWarning($"[Addressables] Safe Release caught: {e.Message}");
                }
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                LoggerService.LogWarning(this, "Releasing null addressable asset");
                return;
            }

            LoggerService.LogDebug(this, $"Releasing addressable asset with name = {instance.name}");
            Addressables.ReleaseInstance(instance);
        }
        
        private void InstantiateAsyncInternal(
            Promise<DisposableAssetInstance> promise,
            AssetReference reference,
            Transform parent = null,
            bool inWorldSpace = false,
            float? timeout = null)
        {
            Debug.Log($"[DEBUG] Starting {nameof(InstantiateAsyncInternal)} Addressables Load for reference: {reference}");
            var handle = Addressables.InstantiateAsync(reference, parent, inWorldSpace);
            
            timeout ??= Timeout;

            handle.OnResult(Promise.Resolved(), _coroutineService, timeout.Value, SendErrorTimeout).Then(either =>
            {
                either.Match(result =>
                    {
                        if (promise.CurState != PromiseState.Pending && result != null)
                        {
                            ReleaseInstance(result);
                            return;
                        }
                    
                        promise.SafeResolve(new DisposableAssetInstance(this, handle, result));
                    },
                    exception =>
                    {
                        ReleaseHandle(handle);
                    
                        if (promise.CurState == PromiseState.Pending)
                        {
                            promise.Reject(exception);
                        }
                    });
            });
        }

        protected override void OnInitialize()
        {
           
        }

        protected override void OnDisposing()
        {
           
        }
    }
}