using System;
using Plugins.FSignal;
using Utilities.Disposable;

namespace Extensions
{
    public static class SignalExtensions
    {
        public static IDisposable MapListener<T>(this FSignal<T> signal, Action<T> action)
        {
            signal.AddListener(action);
            return new DeferredDisposable(() => signal.RemoveListener(action));
        }
    }
}