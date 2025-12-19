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
        
        [Inject] private CoroutineService _coroutineService;
        
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
            if (handle.IsValid())
            {
                try
                {
                    Addressables.Release(handle);
                }
                catch (Exception e)
                {
                    LoggerService.LogError(e);
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

        protected override void OnInitialize()
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposing()
        {
            throw new NotImplementedException();
        }
    }
}