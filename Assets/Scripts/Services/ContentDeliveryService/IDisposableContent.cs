using System;

namespace Services.ContentDeliveryService
{
    public interface IDisposableContent<out T> : IDisposable where T : UnityEngine.Object
    {
        T Asset { get; }
    }
}