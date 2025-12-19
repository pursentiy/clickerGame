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

        public static IDisposable MapListener<T0, T1>(this FSignal<T0, T1> signal, Action<T0, T1> action)
        {
            signal.AddListener(action);
            return new DeferredDisposable(() => signal.RemoveListener(action));
        }
		
        public static IDisposable MapListener<T0, T1, T2>(this FSignal<T0, T1, T2> signal, Action<T0, T1, T2> action)
        {
            signal.AddListener(action);
            return new DeferredDisposable(() => signal.RemoveListener(action));
        }
    }
}