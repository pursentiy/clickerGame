using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Extensions;
using JetBrains.Annotations;
using Plugins.FSignal;
using RSG;
using Services;
using Services.ContentDeliveryService;
using UnityEngine;
using Utilities;
using Utilities.Disposable;
using Zenject;
using Object = UnityEngine.Object;

namespace Handlers.UISystem.Popups
{
    public class UIPopupsHandler : IDisposeProvider, IPopupHider
    {
        [Inject] private readonly AddressableContentDeliveryService _contentDeliveryService;
        [Inject] private readonly CoroutineService _coroutineService;
        
        public Canvas RootCanvas => _popupsCanvas;
        public bool IsQueueProcessingPopup { get; private set; }
        public bool IsProcessingImmediatePopup { get; private set; }
        
        public FSignal<string> ShownPopupsChanged { get; } = new FSignal<string>();
        public FSignal OnPopupHiddenSignal { get; } = new FSignal();
        public FSignal<UIPopupBase> PopupBeginAppearingSignal { get; } = new FSignal<UIPopupBase>();
        public FSignal<UIPopupBase> PopupFinishedAppearingSignal { get; } = new FSignal<UIPopupBase>();
        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();

        private List<UIPopupShowQuery> _popupsShowQueue;
        
        private Dictionary<Type, List<(UIPopupBase popup, IDisposableContent<GameObject> asset)>> _shownPopups;
        private Canvas _popupsCanvas;

        public void Initialize(Canvas parentCanvas)
        {
            this.Inject();
            _popupsShowQueue = new List<UIPopupShowQuery>();
            _shownPopups = new Dictionary<Type, List<(UIPopupBase popup, IDisposableContent<GameObject> asset)>>();
            _popupsCanvas = parentCanvas;
            
            IsQueueProcessingPopup = false;
        }
        
        public IPromise<UIPopupBase> ShowPopupImmediately(Type type, IPopupContext context)
        {
            LoggerService.LogDebug(this, $"ShowPopupImmediately with {nameof(Type)} = {type.Name}");
            //TODO ADD UI BLOCKER
            //var blockRef = _uiBlocker.Block();
            IsProcessingImmediatePopup = true;
            var promise = ShowPopup(type, context);
            promise.Finally(() =>
            {
                IsProcessingImmediatePopup = false;
                // if (blockRef?.IsDisposed == false)
                // {
                //     blockRef.Dispose();
                // }
            });
            return promise;
        }

        public IPromise<TPopup> ShowPopupImmediatelySingleton<TPopup>(IPopupContext context) where TPopup : UIPopupBase
        {
            LoggerService.LogDebug(this, $"ShowPopupImmediatelySingleton executed");
            var popup = this.GetFirst<TPopup>();
            if (popup)
            {
                return Promise<TPopup>.Resolved(popup);
            }
            
            return ShowPopupImmediately<TPopup>(context).Catch(LoggerService.LogError);
        }

        public IPromise<UIPopupBase> ShowPopupImmediatelySingleton(Type type, IPopupContext context)
        {
            LoggerService.LogDebug(this, $"ShowPopupImmediatelySingleton executed");
            var popup = this.GetFirst(type);
            return popup == null ? ShowPopupImmediately(type, context) : Promise<UIPopupBase>.Resolved(popup);
        }

        public IPromise<TPopup> ShowPopupImmediately<TPopup>(IPopupContext context) where TPopup : UIPopupBase
        {
            LoggerService.LogDebug(this, $"ShowPopupImmediately with {nameof(TPopup)} = {typeof(TPopup).Name}");
            return ShowPopupImmediately(typeof(TPopup), context).Then(p => p as TPopup);
        }

        public IPromise<TPopup> EnqueuePopup<TPopup>(IPopupContext context, int overridePriority = -1)
            where TPopup : UIPopupBase
        {
            var promise = new Promise<TPopup>();
            EnqueuePopup(typeof(TPopup), context, overridePriority).Then(popup => promise.Resolve(popup as TPopup));
            return promise;
        }

        public IPromise<UIPopupBase> EnqueuePopupSingleton(Type type, IPopupContext context, int overridePriority = -1)
        {
            var popup = this.GetFirst(type);
            return popup == null ? EnqueuePopup(type, context, overridePriority) : Promise<UIPopupBase>.Resolved(popup);
        }

        public IPromise<TPopup> EnqueuePopupSingleton<TPopup>(IPopupContext context, int overridePriority = -1)
            where TPopup : UIPopupBase
        {
            var popup = this.GetFirst<TPopup>();
            return popup == null ? EnqueuePopup<TPopup>(context, overridePriority) : Promise<TPopup>.Resolved(popup);
        }

        public void HideAllPopups()
        {
            foreach (var list in _shownPopups.Values)
            {
                foreach (var uiPopupBase in list.ToArray())
                {
                    uiPopupBase.popup.Hide();
                }
            }
            
            ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
        }
        
        public void HideAllPopupsExcept(Type type)
        {
            if (type == null)
            {
                HideAllPopups();
                return;
            }
            
            foreach (var list in _shownPopups.Values)
            {
                foreach (var uiPopupBase in list.ToArray())
                {
                    if (uiPopupBase.popup.GetType() == type) 
                        continue;
                    
                    uiPopupBase.popup.Hide();
                }
            }
            
            ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
        }

        public void HideAllPopups(Type type)
        {
            GetShownPopups(type)?.Foreach(p => p.Hide());
        }

        public IPromise<UIPopupBase> EnqueuePopup(Type type, IPopupContext context, int overridePriority = -1)
        {
            var promise = new Promise<UIPopupBase>();
            InsertPopupToQueue(type, context,
                overridePriority > 0
                    ? overridePriority
                    : GetDefaultPriority(type),
                promise);
            ExecutePopupsQuery();
            return promise;
        }

        private int GetDefaultPriority(Type type)
        {
            return type.TryGetAttribute<PriorityAttribute>()?.Priority ?? 0;
        }

        public bool IsPopupsQueueBusy()
        {
            return IsQueueProcessingPopup || IsAnyPopupEnqueued() || IsAnyPopupShown();
        }

        public bool IsAnyPopupEnqueued()
        {
            return _popupsShowQueue.Count > 0;
        }
        
        public bool IsAnyPopupOfTypeOtherThanEnqueued<T>()
        {
            return _popupsShowQueue.Count(p => !(p is T)) > 0;
        }
        
        public bool IsAnyPopupShown()
        {
            return _shownPopups.Count > 0 &&
                   _shownPopups.Any(x => !x.Value.IsNullOrEmpty() && x.Value.Any(p => !p.popup.Hidden));
        }
        
        public bool IsAnyOverlappingPopupShown()
        {
            return _shownPopups.Count > 0 &&
                   _shownPopups.Any(x => !x.Value.IsNullOrEmpty()
                                          && x.Value.Any(p => !p.popup.Hidden && p.popup.IsItAnOverlappingPopup));
        }

        public bool IsAnyPopupShown(Type type)
        {
            if (_shownPopups.TryGetValue(type, out var list))
            {
                return list.Count > 0 && list.Any(pair => !pair.popup.Hidden);
            }
            return false;
        }

        public bool IsAnyPopupShown<TPopup>() where TPopup : UIPopupBase
        {
            return IsAnyPopupShown(typeof(TPopup));
        }

        public List<UIPopupBase> GetShownPopups(Type type)
        {
            if (_shownPopups.IsNullOrEmpty())
                return null;

            if (_shownPopups.TryGetValue(type, out var list))
            {
                return list
                    .Where(p => p.popup != null)
                    .Select(p => p.popup).ToList();
            }
            
            return null;
        }

        public List<TPopup> GetShownPopups<TPopup>() where TPopup : UIPopupBase
        {
            var popups = GetShownPopups(typeof(TPopup));
            return popups?.Cast<TPopup>().ToList();
        }

        public List<TPopup> GetShownPopupsDerivedFrom<TPopup>() where TPopup : class
        {
            var shownPopups = _shownPopups
                .SelectMany(pair => pair.Value.Select(p => p.popup as TPopup))
                .Where(popup => popup != null)
                .ToList();

            return shownPopups;
        }

        [CanBeNull]
        public UIPopupBase GetTopVisiblePopup()
        {
            return _shownPopups
                .SelectMany(pair => pair.Value.Select(p => p.popup))
                .Where(popup => popup != null)
                .OrderBy(popup => popup.transform.GetSiblingIndex())
                .LastOrDefault();
        }
        
        private IPromise<UIPopupBase> ShowPopup(Type type, IPopupContext context)
        {
            if (type == null)
            {
                return Promise<UIPopupBase>.Rejected(new ArgumentException("Requested type is null"));
            }
            
            var key = type.TryGetAttribute<AssetKeyAttribute>()?.Key;

            if (key.IsNullOrEmpty())
            {
                var msg = $"Can't find an asset key for popup {type.Name}";
                LoggerService.LogDebug(msg);
                return Promise<UIPopupBase>.Rejected(new NullReferenceException(msg));
            }

            var uniqueId = Guid.NewGuid();
            var loadPromise = LoadPrefabAsync(key);
            
            var resultPromise = loadPromise.Then(asset =>
            {
                if (_popupsCanvas == null)
                {
                    asset?.Dispose();
                    return Promise<(UIPopupBase popup, IDisposableContent<GameObject> asset)>
                        .Canceled();
                }
            
                var popup =
                    UIInstaller.Container.InstantiatePrefabForComponent<UIPopupBase>(asset.Asset,
                        _popupsCanvas.transform);

                if (popup == null)
                {
                    asset.Dispose();
                    var msg = $"Failed to resolve popup mediator with type = {type.Name}";
                    LoggerService.LogDebug(msg);
                    return Promise<(UIPopupBase popup, IDisposableContent<GameObject> asset)>
                        .Rejected(new MissingComponentException(msg));
                }
                
                popup.SetHider(this);

                var rt = (RectTransform) popup.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.localScale = Vector3.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                return Promise<(UIPopupBase popup, IDisposableContent<GameObject> asset)>.Resolved((popup, asset));
            }).Then(pair =>
            {
                return pair.popup.OnCreated(context, uniqueId)
                    .Then(() => OnPopupCreated(type, pair.popup, pair.asset, context))
                    .CancelWith(pair.popup)
                    .Catch(e =>
                    {
                        LoggerService.LogError(e);
                        DestroyPopup(pair.popup, pair.asset);
                    });
            }).CancelWith(this);
            
            return resultPromise;
        }

        private IPromise<IDisposableContent<GameObject>> LoadPrefabAsync(string key)
        {
            return _contentDeliveryService.LoadAssetAsync<GameObject>(key);
        }
        
        private IPromise<UIPopupBase> OnPopupCreated(Type type, UIPopupBase popup, IDisposableContent<GameObject> asset, IPopupContext context)
        {
            if (_shownPopups.ContainsKey(type))
            {
                _shownPopups[type].Add((popup, asset));
            }
            else
            {
                _shownPopups.Add(type,
                    new List<(UIPopupBase popup, IDisposableContent<GameObject> asset)>
                        {(popup, asset)});
            }

            ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
            
            popup.gameObject.SetActive(true);
            var animation = popup.Animation;

            if (animation == null)
            {
                try
                {
                    PopupBeginAppearingSignal.Dispatch(popup);
                    popup.OnBeginShow();
                    popup.OnEndShow();
                    PopupFinishedAppearingSignal.Dispatch(popup);
                    return Promise<UIPopupBase>.Resolved(popup);
                }
                catch (Exception e)
                {
                    DestroyPopup(popup, asset);
                    return Promise<UIPopupBase>.Rejected(e);
                }
            }

            try
            {
                PopupBeginAppearingSignal.Dispatch(popup);
                animation.SetHiddenState();
                popup.OnBeginShow();
                return animation.AnimateShow(context).Then(() =>
                {
                    try
                    {
                        popup.OnEndShow();
                        PopupFinishedAppearingSignal.Dispatch(popup);
                        LoggerService.LogDebug(this, $"ShowPopup with {nameof(Type)} = {type.Name}: Popup shown");
                        return Promise<UIPopupBase>.Resolved(popup);
                    }
                    catch (Exception e)
                    {
                        DestroyPopup(popup, asset);
                        return Promise<UIPopupBase>.Rejected(e);
                    }
                });
            }
            catch (Exception e)
            {
                DestroyPopup(popup, asset);
                return Promise<UIPopupBase>.Rejected(e);
            }
        }

        private void DestroyPopup(UIPopupBase popup, IDisposableContent<GameObject> asset)
        {
            if (_shownPopups.ContainsKey(popup.GetType()))
            {
                _shownPopups[popup.GetType()].RemoveAll(p =>
                {
                    if (p.popup == popup)
                    {
                        p.asset?.Dispose();
                        return true;
                    }

                    return false;
                });
            }
            asset?.Dispose();
            Object.Destroy(popup.gameObject);
            ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
        }
        
        private void InsertPopupToQueue(Type type, IPopupContext context, int priority, Promise<UIPopupBase> promise)
        {
            if (_popupsShowQueue.Count == 0)
            {
                _popupsShowQueue.Add(new UIPopupShowQuery(type, context, priority, promise));
                return;
            }

            var query = new UIPopupShowQuery(type, context, priority, promise);
            var foundedIndex = _popupsShowQueue.FindLastIndex(p => p.Priority >= priority);
            if (foundedIndex < 0)
            {
                _popupsShowQueue.Insert(0, query);
            }
            else if (foundedIndex >= _popupsShowQueue.Count - 1)
            {
                _popupsShowQueue.Add(query);
            }
            else
            {
                _popupsShowQueue.Insert(foundedIndex + 1, query);
            }
        }
        
        private void ExecutePopupsQuery()
        {
            if (IsQueueProcessingPopup)
            {
                return;
            }

            if (_popupsShowQueue.Count > 0)
            {
                IsQueueProcessingPopup = true;
                var query = _popupsShowQueue[0];
                _popupsShowQueue.RemoveAt(0);

                //TODO ADD UIBlocker
                //var blockRef = _uiBlocker.Block();
                ShowPopup(query.PopupType, query.Context)
                    .Then(popup =>
                    {
                        popup.OnHide(() =>
                        {
                            IsQueueProcessingPopup = false;
                            ExecutePopupsQuery();
                        });
                        query.Promise.Resolve(popup);
                    })
                    .Catch(exception =>
                    {
                        IsQueueProcessingPopup = false;
                        ExecutePopupsQuery();
                        query.Promise.Resolve(null);
                    })
                    .Finally(() =>
                    {
                        //TODO ADD UIBlocker
                        // if (blockRef?.IsDisposed == false)
                        //     blockRef.Dispose();
                    });
            }
        }

        public void HidePopup(UIPopupBase popup)
        {
            if (popup == null)
            {
                return;
            }
            
            if (popup.IsHiding || popup.Hidden)
            {
                return;
            }

            LoggerService.LogDebug($"Hide: {popup.GetAnalyticsName()}");

            //TODO ADD BLOCKER
            //var blockRef = _uiBlocker.Block();

            try
            {
                popup.OnBeginHide();
                AnimateHide()
                    .Then(() =>
                    {
                        try
                        {
                            popup.OnEndHide();
                            popup.OnHiding()
                                .ContinueWithResolved(() =>
                                {
                                    try
                                    {
                                        popup.OnDispose();
                                    }
                                    catch (Exception e)
                                    {
                                        LoggerService.LogError(this, $"Fail hiding popup with type = {popup.GetType().Name}: {e}");
                                    }
                                    finally
                                    {
                                        DisposePopup();
                                    }
                                })
                                .CancelWith(this);
                        }
                        catch (Exception e)
                        {
                            LoggerService.LogError(this, $"Fail hiding popup with type = {popup.GetType().Name}: {e}");
                            DisposePopup();
                        }
                    });
            }
            catch (Exception e)
            {
                LoggerService.LogError(this, $"Fail hiding popup with type = {popup.GetType().Name}: {e}");
                DisposePopup();
            }

            return;
            
            IPromise AnimateHide()
            {
                return popup.Animation?.AnimateHide(null) ?? Promise.Resolved();
            }

            void DisposePopup()
            {
                //blockRef?.Dispose();
                RemoveFromShowedPopups();
                if (popup.gameObject != null)
                {
                    Object.Destroy(popup.gameObject);
                }
                OnPopupHiddenSignal.Dispatch();
            }
            
            void RemoveFromShowedPopups()
            {
                if (_shownPopups.IsNullOrEmpty())
                {
                    return;
                }

                var popupType = popup.GetType();
                if (!_shownPopups.TryGetValue(popupType, out var shownPopups) || shownPopups.IsNullOrEmpty())
                {
                    return;
                }

                shownPopups.RemoveAll(p =>
                    {
                        if (p.popup == popup)
                        {
                            p.asset?.Dispose();
                            return true;
                        }

                        return false;
                    });

                ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
            }
        }

        public Promise HidePopupWithDelayedDispose(UIPopupBase popup)
        {
            if (popup == null)
            {
                return (Promise) Promise.Rejected(new Exception("Popup is NULL!"));
            }
            
            if (popup.IsHiding)
            {
                return (Promise) Promise.Rejected(new Exception("Popup is hiding now!"));
            }

            var name = popup.GetType().Name;

            var promise = new Promise(this);
            var animation = popup.Animation;
            IPromise animationPromise; 
            
            if (animation == null)
            {
                try
                {
                    LoggerService.LogDebug($"Hide: {popup.GetAnalyticsName()}");
                    popup.OnBeginHide();
                    popup.OnEndHide();
                }
                catch (Exception e)
                {
                    LoggerService.LogError(this, $"Fail hiding popup with type = {name}: {e}");
                    RemoveFromShowedPopups();
                    if (popup != null && popup.gameObject != null)
                        Object.Destroy(popup.gameObject);
                }
                animationPromise = Promise.Resolved();
            }
            else
            {
                //TODO ADD UIBLOCKER
                //var blockRef = _uiBlocker.Block();
                try
                {
                    LoggerService.LogDebug($"Hide: {popup.GetAnalyticsName()}");
                    popup.OnBeginHide();
                    animationPromise = animation.AnimateHide(null)
                        .Then(() =>
                        {
                            try
                            {
                                popup.OnEndHide();
                                //blockRef?.Dispose();
                            }
                            catch (Exception e)
                            {
                                //blockRef?.Dispose();

                                LoggerService.LogError(this, $"Fail hiding popup with type = {name}: {e}");
                                RemoveFromShowedPopups();
                                Object.Destroy(popup.gameObject);
                            }
                        });
                }
                catch (Exception e)
                {
                    //blockRef?.Dispose();

                    LoggerService.LogError(this, $"Fail hiding popup with type = {name}: {e}");
                    if (popup != null && popup.gameObject != null)
                        Object.Destroy(popup.gameObject);
                    return (Promise)Promise.Rejected(e);
                }
            }

            Promise.All(promise, animationPromise).Then(() =>
            {
                popup.OnDispose();
                RemoveFromShowedPopups();
                if (popup != null && popup.gameObject != null)
                    Object.Destroy(popup.gameObject);
            });
            return promise;

            void RemoveFromShowedPopups()
            {
                if (_shownPopups.ContainsKey(popup.GetType()))
                {
                    _shownPopups[popup.GetType()].RemoveAll(p =>
                    {
                        if (p.popup == popup)
                        {
                            p.asset?.Dispose();
                            return true;
                        }
                        
                        return false;
                    });
                    
                    ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
                }
            }
        }

        public void HideAllPopups<TPopup>()
        {
            HideAllPopups(typeof(TPopup));
        }

        private class UIPopupShowQuery
        {
            public Type PopupType { get; }
            public IPopupContext Context { get; }
            public int Priority { get; }
            public Promise<UIPopupBase> Promise { get; }

            public UIPopupShowQuery(Type popupType, IPopupContext context, int priority, Promise<UIPopupBase> promise)
            {
                PopupType = popupType;
                Context = context;
                Priority = priority;
                Promise = promise;
            }
        }
        
        private static string PopupsQueueStringify(Dictionary<Type, List<(UIPopupBase popup, IDisposableContent<GameObject> asset)>> popups)
        {
            return popups.Where(popup => !popup.Value.IsNullOrEmpty()).Aggregate(string.Empty, (current, popup) => current + (popup.Key.Name + ":" + popup.Value.Count + ";"));
        }

        public void Dispose()
        {
            _popupsShowQueue.Clear();

            var shownPopupsCopy = _shownPopups.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToList());

            foreach (var pair in shownPopupsCopy)
            {
                if (!pair.Value.IsNullOrEmpty())
                {
                    foreach (var tuple in pair.Value)
                    {
                        tuple.asset?.Dispose();
                        if (tuple.popup != null)
                        {
                            try
                            {
                                tuple.popup.OnDispose();
                            }
                            catch (Exception e)
                            {
                                LoggerService.LogWarning($"Error disposing popup: {e}");
                            }
                            Object.Destroy(tuple.popup);
                        }
                    }
                }
            }
            _shownPopups.Clear();
            shownPopupsCopy.Clear();
            
            ShownPopupsChanged.Dispatch(PopupsQueueStringify(_shownPopups));
        }
    }
}