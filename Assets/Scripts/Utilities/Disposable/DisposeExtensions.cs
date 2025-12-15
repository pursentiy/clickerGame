using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using Platform.Common.Utilities.StateMachine;
using RSG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities.Disposable;

namespace Utilities
{
    public static class DisposeExtensions
    {
        public static Material DisposeWith(this Material mat, IDisposeProvider disposeProvider)
        {
            new DeferredDisposable(() => UnityEngine.Object.Destroy(mat)).DisposeWith(disposeProvider);
            return mat;
        }

        public static Material DisposeWith(this Material mat, MonoBehaviour disposeProvider)
        {
            return DisposeWith(mat, disposeProvider.GetDisposeProvider());
        }
        
        public static IDisposeProvider GetDisposeProvider(this MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                return behaviour.RequireComponent<ComponentDisposeProvider>();
            }

            return null;
        }

        public static IDisposeProvider GetDisposeProvider(this GameObject gameObject)
        {
            if (gameObject != null)
            {
                return gameObject.RequireComponent<ComponentDisposeProvider>();
            }
            
            return null;
        }

        public static IStateMachine DisposeWith(this IStateMachine machine, IDisposeProvider provider)
        {
            new DeferredDisposable(machine.Release).DisposeWith(provider);
            return machine;
        }

        public static IStateMachine DisposeWith(this IStateMachine machine, MonoBehaviour provider)
        {
            return machine.DisposeWith(provider.GetDisposeProvider());
        }

        public static IStateSequence CancelWith(this IStateSequence sequence, IDisposeProvider provider)
        {
            new DeferredDisposable(sequence.Cancel).DisposeWith(provider);
            return sequence;
        }

        public static IStateSequence CancelWith(this IStateSequence sequence, MonoBehaviour provider)
        {
            return sequence.CancelWith(provider.GetDisposeProvider());
        }

        public static IDisposable DisposeWith(this IDisposable disposable, MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour != null)
                monoBehaviour.GetDisposeProvider().ChildDisposables.Add(disposable);
            else
                Debug.LogError("monoBehaviour is null upon calling DisposeWith");
            return disposable;
        }

        public static IStateSequence FinishWith(this IStateSequence sequence, IDisposeProvider provider)
        {
            new DeferredDisposable(sequence.Finish).DisposeWith(provider);
            return sequence;
        }
        
        public static IStateSequence FinishWith(this IStateSequence sequence, IStateSequence provider)
        {
            provider.OnFinish(sequence.Finish);
            return sequence;
        }

        public static IStateSequence FinishWith(this IStateSequence sequence, MonoBehaviour provider)
        {
            return sequence.FinishWith(provider.GetDisposeProvider());
        }

        public static TweenerCore<T1, T2, TPlugOptions> KillWith<T1, T2, TPlugOptions>(
            this TweenerCore<T1, T2, TPlugOptions> tweenerCore,
            IDisposeProvider provider) where TPlugOptions : struct, IPlugOptions
        {
            new DeferredDisposable(() => tweenerCore.Kill()).DisposeWith(provider);
            return tweenerCore;
        }
        
        public static TweenerCore<T1, T2, TPlugOptions> KillWith<T1, T2, TPlugOptions>(
            this TweenerCore<T1, T2, TPlugOptions> tweenerCore,
            IStateSequence sequence) where TPlugOptions : struct, IPlugOptions
        {
            sequence.OnFinish(() => tweenerCore.Kill());
            return tweenerCore;
        }
        
        public static Sequence KillWith(
            this Sequence sequence,
            IStateSequence disposeProvider)
        {
            disposeProvider.OnFinish(() => sequence.Kill());
            return sequence;
        }

        public static TweenerCore<T1, T2, TPlugOptions> KillWith<T1, T2, TPlugOptions>(
            this TweenerCore<T1, T2, TPlugOptions> tweenerCore,
            MonoBehaviour provider) where TPlugOptions : struct, IPlugOptions
        {
            return tweenerCore.KillWith(provider.GetDisposeProvider());
        }

        public static Sequence KillWith(this Sequence sequence, IDisposeProvider provider)
        {
            new DeferredDisposable(() => sequence.Kill()).DisposeWith(provider);
            return sequence;
        }

        public static Sequence KillWith(this Sequence sequence, MonoBehaviour provider)
        {
            return sequence.KillWith(provider.GetDisposeProvider());
        }

        public static IList<T> ClearWith<T>(this IList<T> list, IDisposeProvider provider)
        {
            new DeferredDisposable(list.Clear).DisposeWith(provider);
            return list;
        }

        public static IList<T> ClearWith<T>(this IList<T> list, MonoBehaviour provider)
        {
            return list.ClearWith(provider.GetDisposeProvider());
        }

        public static Toggle SubscribeWithDispose(this Toggle toggle, Action<bool> action, IDisposeProvider provider)
        {
            var unityAction = new UnityAction<bool>(action);
            toggle.onValueChanged.AddListener(unityAction);
            new DeferredDisposable(() => toggle.onValueChanged.RemoveListener(unityAction)).DisposeWith(provider);
            return toggle;
        }

        public static Toggle SubscribeWithDispose(this Toggle toggle, Action<bool> action, MonoBehaviour provider)
        {
            return toggle.SubscribeWithDispose(action, provider.GetDisposeProvider());
        }

        public static IPromise<T> DisposeResultWith<T>(this IPromise<T> promise, IDisposeProvider provider)
            where T : IDisposable
        {
            return promise.Then(d => d.DisposeWith(provider));
        }

        public static IPromise<T> DisposeResultWith<T>(this IPromise<T> promise, MonoBehaviour provider)
            where T : IDisposable
        {
            return DisposeResultWith(promise, provider.GetDisposeProvider());
        }
        
        public static IPromise<T> DisposeResultWith<T>(this IPromise<T> promise, GameObject provider)
            where T : IDisposable
        {
            return DisposeResultWith(promise, provider.GetDisposeProvider());
        }
        
        public static IPromise<T> DisposeResultWith<T>(this IPromise<T> promise, Component provider)
            where T : IDisposable
        {
            return DisposeResultWith(promise, provider.gameObject.GetDisposeProvider());
        }
    }
}