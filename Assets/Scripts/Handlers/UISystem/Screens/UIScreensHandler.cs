using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Attributes;
using Handlers.UISystem.Screens.Transitions;
using JetBrains.Annotations;
using ModestTree;
using Plugins.FSignal;
using RSG;
using Services;
using Services.ContentDeliveryService;
using Services.CoroutineServices;
using Services.ScreenBlocker;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using Object = UnityEngine.Object;
using Extensions;
using UI.Screens.WelcomeScreen;

namespace Handlers.UISystem.Screens
{
    [UsedImplicitly]
    public sealed class UIScreensHandler
    {
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly AddressableContentDeliveryService _contentService;
        [Inject] private readonly PersistentCoroutinesService _persistentCoroutinesService;
        
        private readonly CancelToken _disposables = new();

        private Stack<ScreenTransitionInfo> _screenStack;
        private Action<IEnumerator> _coroutineCallback;
        private bool _shouldFinishTransitionImmediately;
        private IDisposable _screenTransitionDisposable;
        private Func<UIScreenTransition> _fallbackScreenTransition;
        private UIScreen _startScreen;

        public FSignal<string> ScreenStackChanged { get; } = new();
        public FSignal<UIScreenTransition> ScreenTransitionStarted { get; } = new();
        public FSignal<UIScreenTransition> ScreenTransitionEnded { get; } = new();
        public FSignal<UIScreenBase> ScreenStartsAppearing { get; } = new();
        public FSignal<UIScreenBase> ScreenFinishedAppearing { get; } = new();
        public FSignal<Type> OnScreenDestroyedSignal { get; } = new();
        public event Action<Exception> OnFatalError;
        
        public Canvas ScreensCanvas { get; private set; }
        public bool TransitionInProgress { get; private set; }
        
        public UIScreenBase CurrentScreen => CurrentScreenInternal?.ScreenBase;
        public UIScreenBase PreviousScreen => PreviousScreenInternal?.ScreenBase;
        
        public IReadOnlyCollection<UIScreenBase> StackScreens => _screenStack
            .Where(screen => screen is { Screen: not null } && screen.Screen.ScreenBase != null)
            .Select(transition => transition.Screen.ScreenBase)
            .ToArray();
        public bool IsCurrentScreenCalledManually { get; private set; }

        private UIScreen DefaultScreen => GetDefaultScreen();
        private UIScreen PreviousScreenInternal => _screenStack.Count < 2 ? null : _screenStack.ElementAt(1).Screen;
        private UIScreen CurrentScreenInternal => _screenStack == null || _screenStack.Count == 0 ? null : _screenStack.Peek().Screen;

        
        public void Initialize(Canvas parentCanvas, Action<IEnumerator> coroutineCallback)
        {
            _screenStack = new Stack<ScreenTransitionInfo>();
            _coroutineCallback = coroutineCallback;
            
            ScreensCanvas = parentCanvas;
            TransitionInProgress = false;
        }

        public void SetFallbackScreenTransition(Func<UIScreenTransition> fallbackScreenTransition)
        {
            _fallbackScreenTransition = fallbackScreenTransition;
        }

        public void FinishCurrentTransitionImmediately()
        {
            _shouldFinishTransitionImmediately = true;
        }

        public IPromise PushFirstScreen<T>()
        {
            var blockRef = _uiScreenBlocker.Block();
            var promise = PrepareScreen(typeof(T), null, false)
                .Then(PushFirstScreenRoutine)
                .Catch(e =>
                {
                    //FATAL ERROR
                    LoggerService.LogWarning(this, e.Message);
                });
            
            promise.Finally(
                () =>
                {
                    if (blockRef?.IsDisposed == false)
                        blockRef.Dispose();
                });

            return promise;
        }

        public IPromise<UIScreenBase> ChangeScreen(UIScreenTransition transition, bool isScreenCalledManually = true)
        {
            if (TransitionInProgress)
            {
                return Promise<UIScreenBase>.Rejected(new Exception("Screens handler is busy"));
            }
            
            TransitionInProgress = true;
            
            ScreenTransitionStarted.Dispatch(transition);

            var promise = new Promise<UIScreenBase>();

            var unblock = transition.PrepareBeforeTransition(true);

            
            unblock.Then(() =>
                {
                    var blockRef = _uiScreenBlocker.Block();
                    var preparationPromise = PrepareScreen(transition.ToScreenType, transition.Context, transition.DownloadPrefabWithProgress);
                    
                    preparationPromise.Then(newScreen =>
                        {

                            ChangeScreenRoutine(transition, newScreen, isScreenCalledManually)
                                .Finally(() =>
                                {
                                    if (blockRef?.IsDisposed == false)
                                    {
                                        blockRef.Dispose();
                                    }

                                    TransitionInProgress = false;

                                    ScreenTransitionEnded.Dispatch(transition);
                                    
                                    promise.Resolve(newScreen.ScreenBase);
                                });
                        });

                    return preparationPromise;
                })
                .Catch(e =>
                {
                    OnFatalError?.Invoke(e);
                    promise.RejectSilent(e);
                });

            return promise;
        }

        public IPromise<UIScreenBase> SafePushScreen(UIScreenTransition transition, bool fromScreenInactive = true, bool isScreenCalledManually = true)
        {
            if (CurrentScreen != null && CurrentScreen.GetType() == transition.ToScreenType)
                return Promise<UIScreenBase>.Canceled();

            return PushScreen(transition, fromScreenInactive, isScreenCalledManually);
        }

        public IPromise<UIScreenBase> PushScreen(UIScreenTransition transition, bool fromScreenInactive = true, bool isScreenCalledManually = true)
        {
            // if (_screenStack.Count > 0 && _screenStack.Peek().Screen == _startScreen)
            // {
            //     LoggerService.LogWarningEditor($"{nameof(PushScreen)}: Attempt to PushScreen over start screen");
            //     return ChangeScreen(transition, isScreenCalledManually);
            // }
            
            if (TransitionInProgress)
            {
                return Promise<UIScreenBase>.Rejected(new Exception("ScreensHandler is busy"));
            }

            TransitionInProgress = true;
            
            ScreenTransitionStarted.Dispatch(transition);

            var promise = new Promise<UIScreenBase>();
            
            var unblock = transition.PrepareBeforeTransition(true);
            
            unblock.Then(() =>
                { 
                    var blockRef = _uiScreenBlocker.Block();
                    var preparationPromise = PrepareScreen(transition.ToScreenType, transition.Context, transition.DownloadPrefabWithProgress);

                    preparationPromise
                        .Then(newScreen =>
                        {
                            return PushScreenRoutine(transition, fromScreenInactive, newScreen, isScreenCalledManually)
                                .ContinueWithResolved(() =>
                                {
                                    if (blockRef?.IsDisposed == false)
                                    {
                                        blockRef.Dispose();
                                    }

                                    TransitionInProgress = false;

                                    ScreenTransitionEnded.Dispatch(transition);

                                    promise.Resolve(newScreen.ScreenBase);
                                });
                        });

                    return preparationPromise;
                })
                .Catch(e =>
                {
                    OnFatalError?.Invoke(e);
                    promise.RejectSilent(e);
                });

            return promise;
        }
        
        public IPromise<UIScreenBase> PushScreenClean(UIScreenTransition transition, bool fromScreenInactive = true, bool isScreenCalledManually = true)
        {
            if (_screenStack.Count > 0 && _screenStack.Peek().Screen == _startScreen)
            {
                LoggerService.LogWarningEditor($"{nameof(PushScreen)}: Attempt to PushScreen over start screen");
                return ChangeScreen(transition, isScreenCalledManually);
            }
            
            if (TransitionInProgress)
            {
                return Promise<UIScreenBase>.Rejected(new Exception("ScreensHandler is busy"));
            }

            TransitionInProgress = true;
            
            ScreenTransitionStarted.Dispatch(transition);

            var promise = new Promise<UIScreenBase>();
            
            var unblock = transition.PrepareBeforeTransition(true);

            unblock.Then(() =>
                { 
                    var blockRef = _uiScreenBlocker.Block();
                    var preparationPromise = PrepareScreen(transition.ToScreenType, transition.Context, transition.DownloadPrefabWithProgress);
                    
                    preparationPromise
                        .Then(newScreen =>
                        {
                            var screenToDestroy = _screenStack.Count == 0 ? null : _screenStack.Peek();
                            
                            return PushScreenRoutine(transition, fromScreenInactive, newScreen, isScreenCalledManually)
                                .ContinueWithResolved(() =>
                                {
                                    if (blockRef?.IsDisposed == false)
                                    {
                                        blockRef.Dispose();
                                    }

                                    if (screenToDestroy != null)
                                    {
                                        DestroyScreen(screenToDestroy.Screen);
                                        screenToDestroy.Screen = null;
                                    }

                                    TransitionInProgress = false;

                                    ScreenTransitionEnded.Dispatch(transition);
                                
                                    promise.Resolve(newScreen.ScreenBase);
                                });
                		});

                    return preparationPromise;
                })
                .Catch(e =>
                {
                    OnFatalError?.Invoke(e);
                    promise.RejectSilent(e);
                });

            return promise;
        }

        public IPromise SafeTransitionPopScreen(bool isScreenCalledManually = true)
        {
            if (!TransitionInProgress)
                return PopScreen(isScreenCalledManually);

            var safePopScreenPromise = new Promise(this);
            _screenTransitionDisposable = ScreenTransitionEnded
                .MapListener(_ => PopScreen().Finally(safePopScreenPromise.SafeResolve))
                .DisposeWith(_disposables);

            return safePopScreenPromise;
        }

        public IPromise PopScreen(bool isScreenCalledManually = true)
        {
            _screenTransitionDisposable?.Dispose();

            if (TransitionInProgress)
            {
                return Promise.Rejected(new Exception("Busy!"));
            }

            TransitionInProgress = true;

            var info = _screenStack.Pop();
            ScreenStackChanged.Dispatch(ScreensStackStringify(_screenStack));
            
            ScreenTransitionStarted.Dispatch(info.Transition);
            
            var fromScreen = info?.Screen;
            
            var unblock = info.Transition != null
                ? info.Transition.PrepareBeforeTransition(false)
                : Promise.Resolved();

            return unblock
                .Then(() => GetPopUpScreenWithFallback(false))
                .Then(toScreen =>
                {
                    if (fromScreen == null && toScreen == null)
                    {
                        var e = new NullReferenceException($"{nameof(PopScreen)}: FromScreen and ToScreen are null");
                        OnFatalError?.Invoke(e);
                        return Promise.Rejected(e);
                    }

                    if (fromScreen == null)
                    {
                        LoggerService.LogWarningEditor($"{nameof(PopScreen)}: From screen is null");

                        fromScreen = DefaultScreen;

                        if (fromScreen == null)
                        {
                            var e = new NullReferenceException($"{nameof(PopScreen)}: Default screen is null");
                            OnFatalError?.Invoke(e);
                            return Promise.Rejected(e);
                        }
                    }

                    if (toScreen == null)
                    {
                        LoggerService.LogWarningEditor($"{nameof(PopScreen)}: To screen is null");

                        toScreen = DefaultScreen;

                        if (toScreen == null)
                        {
                            var e = new NullReferenceException($"{nameof(PopScreen)}: DefaultScreen is null");
                            OnFatalError?.Invoke(e);
                            return Promise.Rejected(e);
                        }
                    }

                    toScreen.ScreenBase.gameObject.SetActive(true);

                    try
                    {
                        fromScreen.ScreenBase.OnPrepareHide();
                        fromScreen.ScreenBase.OnBeginHide();
                        
                        toScreen.ScreenBase.OnBeginShow();

                        ScreenStartsAppearing.Dispatch(toScreen.ScreenBase);
                    }
                    catch (Exception e)
                    {
                        OnFatalError?.Invoke(e);
                        return Promise.Rejected(e);
                    }

                    
                    //TODO MAYBE CALL THIS 
                    // if (QualityService.AdditionalPrecautionsInScreensTransition)
                    //     GC.Collect();

                    var promise = new Promise(this);

                    var blockRef = _uiScreenBlocker.Block();
                    DoTransition(toScreen.ScreenBase, fromScreen.ScreenBase, info.Transition, false)
                        .Catch(exception =>
                        {
                            OnFatalError?.Invoke(exception);
                            promise.Reject(exception);
                        })
                        .Then(() =>
                        {
                            //TODO MAYBE CALL THIS 
                            // if (QualityService.AdditionalPrecautionsInScreensTransition)
                            //     return _persistentCoroutinesService.WaitFor(0.1f);
                            return Promise.Resolved();
                        })
                        .Then(() =>
                        {
                            info.Transition?.OnTransitionEnded(false);
                            toScreen.ScreenBase.OnEndShow();
                            fromScreen.ScreenBase.OnEndHide();
                        })
                        .Then(() => _persistentCoroutinesService.WaitFrames(2))
                        .Then(() =>
                        {
                            //TODO MAYBE CALL THIS 
                            // if (QualityService.AdditionalPrecautionsInScreensTransition)
                            //     GC.Collect();
                        })
                        .Then(() => DestroyScreen(fromScreen))
                        .Then(() => _persistentCoroutinesService.WaitFrames(2))
                        .Then(() =>
                        {
                            //TODO MAYBE CALL THIS 
                            // if (QualityService.AdditionalPrecautionsInScreensTransition)
                            //     return _persistentCoroutinesService.WaitFor(0.1f);
                            return Promise.Resolved();
                        })
                        .Then(() =>
                        {
                            try
                            {
                                ScreenFinishedAppearing.Dispatch(toScreen.ScreenBase);
                                IsCurrentScreenCalledManually = isScreenCalledManually;

                                TransitionInProgress = false;

                                if (blockRef?.IsDisposed == false)
                                {
                                    blockRef.Dispose();
                                }

                                ScreenTransitionEnded.Dispatch(info.Transition);

                                promise.Resolve();
                            }
                            catch (Exception e)
                            {
                                OnFatalError?.Invoke(e);
                                promise.Reject(e);
                            }
                        });

                    return promise;
                });
        }
        
        public void Dispose()
        {
            foreach (var info in _screenStack.ToList())
            {
                if (info.Screen.ScreenBase != null)
                {
                    try
                    {
                        info.Screen.ScreenBase.OnDispose();
                    }
                    catch (Exception e)
                    {
                        LoggerService.LogWarningEditor($"Error disposing screen: {e}");
                    }
                    Object.Destroy(info.Screen.ScreenBase);
                }
                info.Screen.Asset?.Dispose();
            }

            _screenStack.Clear();
            _disposables.Cancel();
        }

        private IPromise<UIScreen> GetPopUpScreenWithFallback(bool downloadPrefabWithProgress)
        {
            if (_screenStack.Count == 0)
            {
                var transition = _fallbackScreenTransition.Invoke();
                
                return PrepareScreen(transition.ToScreenType, transition.Context, downloadPrefabWithProgress)
                    .Then(newScreen =>
                    {
                        _screenStack.Push(new ScreenTransitionInfo(transition, newScreen));
                        return Promise<UIScreen>.Resolved(newScreen);
                    });
            }
            
            var info = _screenStack.Peek();
            if (info.Screen != null)
                return Promise<UIScreen>.Resolved(info.Screen);

            return PrepareScreen(info.ScreenType, info.Context, downloadPrefabWithProgress)
                .Then(screen => info.Screen = screen);
        }
    
        private IPromise<UIScreen> PrepareScreen(Type type, IScreenContext context, bool downloadPrefabWithProgress)
        {
            var currentScreen = CurrentScreenInternal;
            if (currentScreen != null && currentScreen.ScreenBase != null)
            {
                try
                {
                    currentScreen.ScreenBase.OnPrepareHide();
                }
                catch (Exception e)
                {
                    OnFatalError?.Invoke(e);
                    return Promise<UIScreen>.Rejected(e);
                }
            }

            var key = type.TryGetAttribute<AssetKeyAttribute>()?.Key;
            if (key.IsNullOrEmpty())
            {
                var e = new NullReferenceException($"Can't find an asset key for screen {type.Name}");
                OnFatalError?.Invoke(e);
                return Promise<UIScreen>.Rejected(e);
            }

            var watch = new Stopwatch();
            watch.Start();
            
            return LoadPrefabAsync(key, downloadPrefabWithProgress, type)
                .Then(asset =>
                {
                    
                    watch.Stop();
                    
                    if (ScreensCanvas == null)
                    {
                        asset?.Dispose();
                        return Promise<UIScreen>.Canceled();
                    }
                    
                    watch.Restart();
                    
                    var toScreen = UIInstaller.Container.InstantiatePrefabForComponent<UIScreenBase>(asset.Asset, ScreensCanvas.transform);
                    
                    watch.Stop();
                    
                    if (toScreen == null)
                    {
                        var e = new MissingComponentException($"Mediator for screen type = {type.Name} is null");
                        OnFatalError?.Invoke(e);
                        asset.Dispose();
                        return Promise<UIScreen>.Rejected(e);
                    }

                    var rt = toScreen.transform as RectTransform;
                    if (rt == null)
                    {
                        var e = new NullReferenceException($"RectTransform for screen type = {type.Name} is null");
                        OnFatalError?.Invoke(e);
                        asset.Dispose();
                        return Promise<UIScreen>.Rejected(e);
                    }
                    
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.localScale = Vector3.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    
                    var waitPromise = Promise.Resolved();
                    //TODO CHEATS
    //                 
    // #if CHEATS
    //                 var delay = PlayerPrefs.GetFloat("cheat_EmulateUISystemDelay");
    //                 if (!delay.ApproxZero() && delay > 0)
    //                 {
    //                     waitPromise = ContainerHolder.CurrentContainer.Resolve<ICoroutineService>().WaitFor(delay);
    //                 }
    // #endif
                    
                    watch.Restart();

                    var result = toScreen
                        .OnCreated(context)
                        .Then(() => waitPromise)
                        .Then(() => Promise<UIScreen>.Resolved(new UIScreen(toScreen, asset)))
                        .Catch(e =>
                        {
                            asset.Dispose();
                            OnFatalError?.Invoke(e);
                        });
                    
                    result.Finally(() =>
                    {
                        watch.Stop();
                    });

                    return result;
                })
                .Catch(e =>
                {
                    OnFatalError?.Invoke(e);
                });
        }
        
        private IPromise<IDisposableContent<GameObject>> LoadPrefabAsync(string key, bool downloadPrefabWithProgress, Type screenType)
        {
            return LoadPrefab(key, screenType);
        }

        private IPromise<IDisposableContent<GameObject>> LoadPrefab(string key, Type type)
        {
            var promise = _contentService.LoadAssetAsync<GameObject>(key);
            return promise;
        }
        
        private IPromise PushFirstScreenRoutine(UIScreen startScreen)
        {
            if (startScreen == null)
            {
                LoggerService.LogError(new NullReferenceException("Start screen is null!"));
                return Promise.Rejected(new NullReferenceException());
            }
            
            _screenStack.Push(new ScreenTransitionInfo(null, startScreen));
            ScreenStackChanged.Dispatch(ScreensStackStringify(_screenStack));

            try
            {
                startScreen.ScreenBase.OnBeginShow();
                startScreen.ScreenBase.OnAppearProgress(1);
                startScreen.ScreenBase.OnEndShow();
            }
            catch (Exception e)
            {
                startScreen.Asset?.Dispose();
                OnFatalError?.Invoke(e);
                return Promise.Rejected(e);
            }
            startScreen.ScreenBase.gameObject.SetActive(true);
            _startScreen = startScreen;
            return Promise.Resolved();
        }
        
        private IPromise ChangeScreenRoutine(UIScreenTransition transition, UIScreen newScreen, bool isScreenCalledManually)
        {
            if (newScreen == null)
            {
                var e = new NullReferenceException($"{nameof(ChangeScreen)}: New screen is null");
                OnFatalError?.Invoke(e);
                return Promise.Rejected(e);
            }
            
            var fromScreen = CurrentScreenInternal;
            if (fromScreen == null)
            {
                LoggerService.LogWarningEditor($"{nameof(ChangeScreen)}: From screen is null");

                fromScreen = DefaultScreen;
                if (fromScreen == null)
                {
                    var e = new NullReferenceException($"{nameof(ChangeScreen)}: Default screen is null");
                    OnFatalError?.Invoke(e);
                    return Promise.Rejected(e);
                }
            }

            var promise = new Promise(this);

            var poppedTransition = _screenStack.Pop();
            var newTransitionInfoTransition = poppedTransition.Screen == _startScreen
                ? transition
                : poppedTransition.Transition;
            _screenStack.Push(new ScreenTransitionInfo(newTransitionInfoTransition, newScreen));
            
            ScreenStackChanged.Dispatch(ScreensStackStringify(_screenStack));

            try
            {
                fromScreen.ScreenBase.OnBeginHide();

                newScreen.ScreenBase.OnBeginShow();

                ScreenStartsAppearing.Dispatch(newScreen.ScreenBase);
            }
            catch (Exception e)
            {
                OnFatalError?.Invoke(e);
                promise.RejectSilent(e);
                return promise;
            }

            DoTransition(fromScreen.ScreenBase, newScreen.ScreenBase, transition, true)
                .Catch(exception =>
                {
                    OnFatalError?.Invoke(exception);
                    promise.Reject(exception);
                })
                .Then(() =>
                {
                    try
                    {
                        transition?.OnTransitionEnded(true);
                        fromScreen.ScreenBase.OnEndHide();
                        DestroyScreen(fromScreen);

                        newScreen.ScreenBase.OnEndShow();
                        ScreenFinishedAppearing.Dispatch(newScreen.ScreenBase);
                        IsCurrentScreenCalledManually = isScreenCalledManually;

                        promise.Resolve();
                    }
                    catch (Exception e)
                    {
                        OnFatalError?.Invoke(e);
                        promise.RejectSilent(e);
                    }
                });

            return promise;
        }

        private IPromise PushScreenRoutine(
            UIScreenTransition transition, 
            bool fromScreenInactive,
            UIScreen newScreen, 
            bool isScreenCalledManually)
        {
            var fromScreen = CurrentScreenInternal;
            
            if (newScreen == null)
            {
                var e = new NullReferenceException($"{nameof(PushScreen)}: New screen is null");
                OnFatalError?.Invoke(e);
                return Promise.Rejected(e);
            }

            if (fromScreen == null)
            {
                LoggerService.LogWarningEditor($"{nameof(PushScreen)}: From screen is null");

                fromScreen = DefaultScreen;

                if (fromScreen == null)
                {
                    var e = new NullReferenceException($"{nameof(PushScreen)}: DefaultScreen is null");
                    OnFatalError?.Invoke(e);
                    return Promise.Rejected(e);
                }
            }
            
            var promise = new Promise(this);

            _screenStack.Push(new ScreenTransitionInfo(transition, newScreen));
            ScreenStackChanged.Dispatch(ScreensStackStringify(_screenStack));

            newScreen.ScreenBase.gameObject.SetActive(true);

            try
            {
                fromScreen.ScreenBase.OnBeginHide();
                
                newScreen.ScreenBase.OnBeginShow();

                ScreenStartsAppearing.Dispatch(newScreen.ScreenBase);
            }
            catch (Exception e)
            {
                OnFatalError?.Invoke(e);
                promise.RejectSilent(e);
                return promise;
            }

            DoTransition(fromScreen.ScreenBase, newScreen.ScreenBase, transition, true)
                .Catch(exception =>
                {
                    OnFatalError?.Invoke(exception);
                    promise.Reject(exception);
                })
                .Then(() =>
                {
                    try
                    {
                        transition?.OnTransitionEnded(true);
                        fromScreen.ScreenBase.OnEndHide();
                        newScreen.ScreenBase.OnEndShow();

                        fromScreen.ScreenBase.gameObject.SetActive(!fromScreenInactive);
                        
                        ScreenFinishedAppearing.Dispatch(newScreen.ScreenBase);
                        IsCurrentScreenCalledManually = isScreenCalledManually;

                        promise.Resolve();
                    }
                    catch (Exception e)
                    {
                        OnFatalError?.Invoke(e);
                        promise.Reject(e);
                    }
                });

            return promise;
        }
        
        private IPromise DoTransition(UIScreenBase fromScreen, UIScreenBase toScreen, UIScreenTransition transition, bool forward)
        {
            if (_coroutineCallback == null)
            {
                return Promise.Rejected(new Exception("Coroutine provider callback is null!"));
            }

            if (transition is UIScreenTimeBasedTransitionBase timeBasedTransition)
            {
                var promise = new Promise(this);
                _coroutineCallback.Invoke(DoTransitionRoutine(promise, timeBasedTransition, fromScreen, toScreen, forward));
                return promise;   
            }

            if (transition is UIScreenPromiseBasedTransitionBase promiseBasedTransition)
            {
                return promiseBasedTransition.DoTransition(fromScreen, toScreen, forward);
            }
            
            return Promise.Rejected(new Exception("Transition is null!"));
        }
    
        private IEnumerator DoTransitionRoutine(
            Promise promise,
            UIScreenTimeBasedTransitionBase transitionBase,
            UIScreenBase fromScreen,
            UIScreenBase toScreen,
            bool forward)
        {
            if (fromScreen == null)
            {
                var e = new NullReferenceException("FromScreen is null");
                promise.Reject(e);
                yield break;
            }

            if (toScreen == null)
            {
                var e = new NullReferenceException("ToScreen is null");
                promise.Reject(e);
                yield break;
            }
            
            var animateT = 0f;
            
            try
            {
                (forward ? fromScreen : toScreen).OnDisappearProgress(animateT);
                (forward ? toScreen : fromScreen).OnAppearProgress(animateT);
                transitionBase?.Prepare(fromScreen, toScreen, forward);
            }
            catch (Exception e)
            {
                promise.Reject(e);
                yield break;
            }

            var totalTime = transitionBase?.TransitionTime ?? 0;
            var t = forward ? 0f : totalTime;
            var deltaTimeModifier = 1f;

            while (forward ? t < totalTime : t > 0)
            {
                var slowDown = transitionBase?.SlowDown ?? false;
                
                if (_shouldFinishTransitionImmediately)
                {
                    deltaTimeModifier = 1000000f;
                    slowDown = false;
                }
                
                var multiplier = slowDown ? 0.01f : 1f;
                t += (forward ? 1f : -1f) * (Time.deltaTime * deltaTimeModifier * multiplier);
                t = Mathf.Clamp(t, 0, totalTime);
                animateT = Mathf.Clamp01(t / totalTime);

                try
                {
                    transitionBase?.DoTransition(animateT, forward);
                    (forward ? fromScreen : toScreen).OnDisappearProgress(animateT);
                    (forward ? toScreen : fromScreen).OnAppearProgress(animateT);
                }
                catch (Exception e)
                {
                    promise.Reject(e);
                    yield break;
                }

                yield return null;
            }

            try
            {
                transitionBase?.OnComplete(fromScreen, toScreen, forward);
            }
            catch (Exception e)
            {
                promise.Reject(e);
                yield break;
            }

            _shouldFinishTransitionImmediately = false;
            
            if (transitionBase != null)
            {
                var blocked = true;

                transitionBase.EnsureContent(forward).Finally(() =>
                {
                    blocked = false;
                });

                while (blocked)
                {
                    yield return 0;
                }
            }

            promise.Resolve();
        }

        private void DestroyScreen(UIScreen screen)
        {
            if (screen == null)
            {
                return;
            }
            
            if (screen == _startScreen)
            {
                _startScreen = null;
            }

            try
            {
                OnScreenDestroyedSignal.Dispatch(screen.ScreenBase.GetType());
                screen.ScreenBase.OnDispose();
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(this, $"Error disposing screen type = {screen.GetType().Name}: {e}");
            }
            finally
            {
                screen.Asset?.Dispose();
                Object.Destroy(screen.ScreenBase.gameObject);   
            }
        }
        
        private UIScreen GetDefaultScreen()
        {
            return _screenStack.FirstOrDefault(trans => trans.Screen.ScreenBase is WelcomeScreenMediator)?.Screen;
        }
        
        private string ScreensStackStringify(IEnumerable<ScreenTransitionInfo> screens)
        {
            var result = string.Empty;
            foreach (var screen in screens)
            {
                if (screen?.Screen?.ScreenBase != null)
                {
                    result += screen.Screen.ScreenBase.GetType().Name + "<";
                }
            }
            return result;
        }

        private class ScreenTransitionInfo
        {
            public UIScreenTransition Transition { get; }
            public UIScreen Screen { get; set; }
            public IScreenContext Context { get; }
            public Type ScreenType { get; }

            public ScreenTransitionInfo(UIScreenTransition transition, UIScreen screen)
            {
                Screen = screen;
                Transition = transition;
                Context = screen.ScreenBase.GetContext();
                ScreenType = screen.ScreenBase.GetType();
            }
        }
    }
}