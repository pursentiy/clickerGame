using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.ContentDeliveryService
{
    public class DisposableAssetInstance : IDisposableContent<GameObject>
    {
        public GameObject Asset => _instance;
        public AsyncOperationHandle<GameObject> Handle => _handle;
        
        private readonly AddressableContentDeliveryService _deliveryService;

        private GameObject _instance;
        private AsyncOperationHandle<GameObject> _handle;
        private bool _disposed = false;

        public DisposableAssetInstance(
            AddressableContentDeliveryService deliveryService,
            AsyncOperationHandle<GameObject> handle,
            GameObject instance)
        {
            _deliveryService = deliveryService;
            _handle = handle;
            _instance = instance;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _deliveryService.ReleaseHandle(_handle);
            _handle = default;
            _instance = null;
        }
    }
}