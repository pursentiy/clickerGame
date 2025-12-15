using System;
using DG.Tweening;
using Extensions;
using Platform.Common.Utilities.StateMachine;
using Plugins.RSG.Promise;
using RSG;
using UnityEngine;

namespace Utilities.Disposable
{
    public static class DisposeService
    {
        public static T DisposeWith<T>(this T disposable, IDisposeProvider provider) where T : IDisposable
        {
            if (provider == null)
            {
                disposable.Dispose();
                return disposable;
            }

            provider.ChildDisposables.Add(disposable);
            return disposable;
        }

        public static T DisposeWith<T>(this T disposable, IPromise promise) where T : IDisposable
        {
            if (promise.CurState != PromiseState.Pending)
            {
                disposable?.Dispose();
                return disposable;
            }

            promise.Finally(() => disposable?.Dispose());
            return disposable;
        }
        
        public static T DisposeWith<T, TPromise>(this T disposable, IPromise<TPromise> promise) where T : IDisposable
        {
            if (promise.CurState != PromiseState.Pending)
            {
                disposable?.Dispose();
                return disposable;
            }

            promise.Finally(() => disposable?.Dispose());
            return disposable;
        }

        public static T DontDisposeWith<T>(this T disposable, IDisposeProvider provider) where T : IDisposable
        {
            provider.ChildDisposables.Remove(disposable);
            return disposable;
        }
        
        public static T DisposeWith<T>(this T disposable, GameObject provider) where T : IDisposable
        {
            if (provider == null)
            {
                disposable.Dispose();
                return disposable;
            }
            
            IDisposeProvider disposer = provider.RequireComponent<ComponentDisposeProvider>();
            return disposable.DisposeWith(disposer);
        }
        
        public static T DisposeWith<T>(this T disposable, MonoBehaviour provider) where T : IDisposable
        {
            if (provider.IsDestroyed())
            {
                disposable.Dispose();
                return disposable;
            }
            
            IDisposeProvider disposer = provider.gameObject.RequireComponent<ComponentDisposeProvider>();
            return disposable.DisposeWith(disposer);
        }
        
        public static T DisposeWith<T>(this T disposable, IStateSequence sequence) where T : IDisposable
        {
            sequence.OnFinish(disposable.Dispose);
            return disposable;
        }
        
        public static T CancelAllChildrenWith<T>(this T cancelable, IDisposeProvider provider) 
            where T : ICancellablePromise
        {
            new DeferredDisposable(cancelable.Cancel).DisposeWith(provider);
            return cancelable;
        }
        
        public static T CancelWith<T>(this T cancelable, IDisposeProvider provider) where T : ICancellablePromise
        {
            new DeferredDisposable(cancelable.Cancel).DisposeWith(provider);
            return cancelable;
        }
        
        public static T CancelWith<T>(this T cancelable, MonoBehaviour provider) where T : ICancellablePromise
        {
            new DeferredDisposable(cancelable.Cancel).DisposeWith(provider);
            return cancelable;
        }
        
        public static T CancelWith<T>(this T cancelable, GameObject provider) where T : ICancellablePromise
        {
            new DeferredDisposable(cancelable.Cancel).DisposeWith(provider);
            return cancelable;
        }

        public static T CancelWith<T>(this T cancelable, IPromise promise) where T : ICancellablePromise
        {
            promise.OnCancel(cancelable.Cancel);
            return cancelable;
        }
        
        public static T CancelWith<T, TPromise>(this T cancelable, IPromise<TPromise> promise) 
            where T : ICancellablePromise
        {
            promise.OnCancel(cancelable.Cancel);
            return cancelable;
        }
     
        public static T CancelWith<T>(this T cancelable, IStateSequence sequence) where T : ICancellablePromise
        {
            sequence.OnFinish(cancelable.Cancel);
            return cancelable;
        }
        
        public static T KillWith<T>(this T tween, IDisposeProvider provider) where T : Tween
        {
            new DeferredDisposable(() => { if (tween.IsActive()) tween.Kill(true); }).DisposeWith(provider);
            return tween;
        }

        public static T KillWith<T>(this T tween, MonoBehaviour provider) where T : Tween
        {
            new DeferredDisposable(() =>
            {
                if (tween.IsActive()) 
                    tween.Kill(true);
            }).DisposeWith(provider);
            
            return tween;
        }
        
        public static T KillWith<T>(this T tween, GameObject provider) where T : Tween
        {
            new DeferredDisposable(() =>
            {
                if (tween.IsActive()) 
                    tween.Kill(true);
            }).DisposeWith(provider.GetDisposeProvider());
            
            return tween;
        }
        
        public static void HandledDispose(IDisposeProvider provider)
        {
            provider.ChildDisposables.Dispose();
        }

        public static void HandledDisposeMonoBehaviour(MonoBehaviour provider)
        {
            if (provider == null)
            {
                return;
            }

            var obj = provider.gameObject;

            if (obj == null)
            {
                return;
            }
            
            IDisposeProvider disposer = obj.GetComponent<ComponentDisposeProvider>();
            disposer.May(HandledDispose);
        }
    }
    
    public class CancelToken : IDisposeProvider
    {
        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        public void Cancel()
        {
            DisposeService.HandledDispose(this);
        }
    }
}