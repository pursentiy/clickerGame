using System;
using DG.Tweening;
using RSG;

namespace Extensions
{
    public static class PromiseExtensions
    {
        public static IPromise AsPromiseWithKillOnCancel(this Sequence sequence, bool completeOnKill = false)
        {
            var promise = sequence.AsPromise();
            promise.OnCancel(() => sequence.Kill(completeOnKill));
            return promise;
        }
        
        public static IPromise AsPromise(this Sequence sequence)
        {
            var promise = new Promise(sequence);
            AppendOnComplete(sequence, promise);
            AppendOnKill(sequence, promise);

            if (sequence.IsComplete())
            {
                promise.SafeResolve();
                return promise;
            }

            sequence.OnComplete(promise.SafeResolve);

            return promise;
        }
        
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
        
        private static void AppendOnComplete(Sequence sequence, Promise promise)
        {
            if (sequence.onComplete != null)
            {
                var oldCallback = sequence.onComplete;
                sequence.OnComplete(() =>
                {
                    oldCallback.Invoke();
                    promise.SafeResolve();
                });
            }
            else
            {
                sequence.OnComplete(promise.SafeResolve);
            }
        }
        
        private static void AppendOnComplete(Tweener tweener, Promise promise)
        {
            if (tweener.onComplete != null)
            {
                var oldCallback = tweener.onComplete;
                tweener.OnComplete(() =>
                {
                    oldCallback.Invoke();
                    promise.SafeResolve();
                });
            }
            else
            {
                tweener.OnComplete(promise.SafeResolve);
            }
        }
        
        public static IPromise AsPromiseWithKillOnCancel(this Tweener tweener, bool completeOnKill = false)
        {
            var promise = tweener.AsPromise();
            promise.OnCancel(() => tweener.Kill(completeOnKill));
            return promise;
        }
        
        public static IPromise AsPromise(this Tweener tweener)
        {
            var promise = new Promise(tweener);
            AppendOnComplete(tweener, promise);
            AppendOnKill(tweener, promise);
            return promise;
        }
        
        private static void AppendOnKill(Tweener tweener, Promise promise)
        {
            if (tweener.onKill != null)
            {
                var oldCallback = tweener.onKill;
                tweener.OnKill(() =>
                {
                    oldCallback.Invoke();
                    promise.SafeResolve();
                });
            }
            else
            {
                tweener.OnKill(promise.SafeResolve);
            }
        }
        
        private static void AppendOnKill(Sequence sequence, Promise promise)
        {
            if (sequence.onKill != null)
            {
                var oldCallback = sequence.onKill;
                sequence.OnKill(() =>
                {
                    oldCallback.Invoke();
                    promise.SafeResolve();
                });
            }
            else
            {
                sequence.OnKill(promise.SafeResolve);
            }
        }
    }
}