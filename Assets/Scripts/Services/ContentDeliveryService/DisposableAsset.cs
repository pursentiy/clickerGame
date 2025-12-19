using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.ContentDeliveryService
{
    public sealed class DisposableAsset<T> : IDisposableContent<T> where T : UnityEngine.Object
    {
        public T Asset => _result;
        
        private readonly AddressableContentDeliveryService _deliveryService;
        
        private AsyncOperationHandle<T> _handle;
        private T _result;
        private bool _disposed = false;

        public DisposableAsset(AddressableContentDeliveryService deliveryService, AsyncOperationHandle<T> handle, T result)
        {
            _handle = handle;
            _result = result;
            _deliveryService = deliveryService;
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
            _result = default;
        }
    }
}