using System;
using System.Collections.Generic;
using Extensions;
using Handlers.UISystem.Popups;
using Plugins.FSignal;
using RSG;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Handlers.UISystem
{
    public abstract class UIPopupBase<TView, TContext> : UIPopupBase, IMediator where TView : IUIView where TContext : IPopupContext
    {
        [Inject] private IUIView _view;
        private IPopupContext _context;
        protected TView View => (TView) _view;
        protected TContext Context => (TContext) _context;

        public sealed override IPromise OnCreated(IPopupContext context, Guid uniqueId)
        {
            _context = context;
            return base.OnCreated(context, uniqueId);
        }
    }

    public abstract class UIPopupBase<TView> : UIPopupBase where TView : IUIView
    {
        [Inject] private IUIView _view;
        protected TView View => (TView) _view;
    }
    
    public abstract class UIPopupBase : MonoBehaviour, IUIMediator, IUIPopup
    {
        [Inject] protected readonly SoundHandler SoundHandler;

        private bool _isDisposed;
        
        public FPromisedSignal OnShowEnd { get; } = new FPromisedSignal();
        public Guid UniqueId { get; private set; }
        public bool Hidden { get; private set; }
        public bool IsHiding { get; private set; }
        public double CreationTime { get; private set; }
        public virtual bool CanHideByBackButton => true;
        public virtual bool PlayOnShowSound => true;
        public virtual bool PlayOnHideSound => true;
        public virtual bool IsItAnOverlappingPopup => true;
        
        private readonly List<Action> _onHideCallbacks = new List<Action>();

        protected bool OnCreatedExecuted { get; private set; }
        protected bool IsAppeared { get; private set; }
        
        protected virtual string OnShowSoundKey => AudioExtensions.PopupAppearKey;
        protected virtual string OnHideSoundKey => AudioExtensions.PopupHideKey;
        
        private GraphicRaycaster _graphicRaycaster;
        private IPopupHider _popupHider;

        public FSignal OnBeginHideSignal { get; } = new ();

        public virtual IPromise OnCreated(IPopupContext context, Guid uniqueId)
        {
            try
            {
                UniqueId = uniqueId;
                OnCreated();
            }
            catch (Exception e)
            {
                return Promise.Rejected(e);
            }
            
            return OnCreatedDelayed().Then(() =>
            {
                OnCreatedExecuted = true;

                _graphicRaycaster = GetComponent<GraphicRaycaster>();
                SetRaycasterEnabled(true);
            });
        }

        public void SetCreationTime(double time) => CreationTime = time;

        public void SetHider(IPopupHider popupHider)
        {
            _popupHider = popupHider;
        }

        public virtual void OnDisappearProgress(float progress)
        {
            
        }

        public virtual void OnDispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            var cached = new List<Action>(_onHideCallbacks); 
            _onHideCallbacks.Clear();
            
            foreach (var onHideCallback in cached)
            {
                onHideCallback?.Invoke();
            }
        }

        public virtual void OnCreated()
        {
            if (PlayOnShowSound)
            {
                SoundHandler.PlaySound(OnShowSoundKey);
            }
        }

        public virtual IPromise OnCreatedDelayed()
        {
            return Promise.Resolved();
        }
        

        public virtual void OnBeginShow()
        {
            OnShowEnd.Reset();
            Hidden = false;
            IsHiding = false;
            
        }

        public virtual void OnEndShow()
        {
            IsAppeared = true;
            OnShowEnd.Dispatch();
        }
        
        public virtual void OnPrepareHide()
        {
        
        }

        public virtual void OnBeginHide()
        {
            IsAppeared = false;
            IsHiding = true;
            
            SetRaycasterEnabled(false); 
            
            OnBeginHideSignal?.Dispatch();
        }

        public virtual void OnEndHide()
        {
            if (PlayOnHideSound)
            {
                SoundHandler.PlaySound(OnHideSoundKey);
            }
            
            Hidden = true;
            IsHiding = false;
        }

        public virtual IPromise OnHiding()
        {
            return Promise.Resolved();
        }

        public virtual void OnAppearProgress(float progress)
        {
            
        }

        public virtual string GetAnalyticsName()
        {
            return GetType().Name;
        }
        
        public virtual string GetAnalyticsReason()
        {
            return string.Empty;
        }
        
        public virtual double ActualizeVisibleTime(double elapsedSeconds)
        {
            return elapsedSeconds;
        }

        public abstract IUIPopupAnimation Animation { get; }
        public virtual int Priority { get; } = PopupPriorityLevel.Normal;
        
        /// <summary>
        /// Keep in mind that this method can be invoked from the outside
        /// </summary>
        public virtual void Hide()
        {
            _popupHider.HidePopup(this);
        }
        
        /// <summary>
        /// Keep in mind that this method can be invoked from the outside
        /// </summary>
        public virtual Promise HideWithDelayedDispose()
        {
            return _popupHider.HidePopupWithDelayedDispose(this);
        }

        public void HideByBackButton()
        {
            if (CanHideByBackButton)
            {
                OnHideByBackButton();
            }
        }

        protected virtual void OnHideByBackButton()
        {
            Hide();
        }

        public IUIPopup SubscribeOnHide(Action onHide)
        {
            if (_isDisposed)
            {
                onHide?.Invoke();
                return this;
            }
            
            _onHideCallbacks.Add(onHide);
            return this;
        }

        private void SetRaycasterEnabled(bool isEnable)
        {
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = isEnable;
            }
        }
    }
}

public static class PopupHideExtensions
{
    public static IUIPopup OnHide(this IUIPopup popup, Action onHide)
    {
        return popup.SubscribeOnHide(onHide);
    }
}