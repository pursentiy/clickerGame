using System;
using RSG;

namespace Extensions
{
    public static class PromiseExtensions
    {
        public static void SafeResolve<PromisedT>(this IPendingPromise<PromisedT> promise, PromisedT value)
        {
            if (!promise.CanBeResolved)
            {
                return;
            }

            promise.Resolve(value);
        }

        public static void SafeResolve(this IPendingPromise promise)
        {
            if (!promise.CanBeResolved)
            {
                return;
            }

            promise.Resolve();
        }
        
        public static IPromise OnCanceled(this IPromise promise, Action action)
        {
            promise.OnCancel(action);
            return promise;
        }

        public static IPromise<T> OnCanceled<T>(this IPromise<T> promise, Action action)
        {
            promise.OnCancel(action);
            return promise;
        }
    }
}