using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Plugins.FSignal;
using RSG;
using UnityEngine;
using Zenject;

namespace Handlers.UISystem.Screens
{
    public abstract class UIScreenBase<TView, TContext> : UIScreenBase where TView : IUIView where TContext : IScreenContext
    {
        [Inject] private IUIView _view;
        
        private IScreenContext _context;
        protected TView View => (TView)_view;
        public TContext Context => (TContext)_context;

        public override IScreenContext GetContext()
        {
            return _context;
        }

        public sealed override IPromise OnCreated(IScreenContext context)
        {
            _context = context;
            return base.OnCreated(context);
        }
    }
    
    public abstract class UIScreenBase<TView> : UIScreenBase where TView : IUIView
    {
        [Inject] private IUIView _view;
        
        protected TView View => (TView)_view;

        public override IScreenContext GetContext()
        {
            return null;
        }

        public sealed override IPromise OnCreated(IScreenContext context)
        {
            return base.OnCreated(context);
        }
    }
    
    public abstract class UIScreenBase : MonoBehaviour, IUIMediator, IMediator
    {
        [Inject] private DiContainer _container;
        [Inject] private List<IWidget> _widgets;
        [Inject] private SoundHandler _soundHandler;

        public virtual bool CanResync => false;
        public abstract IScreenContext GetContext();
        
        public bool CompletelyAppeared { get; private set; }
        public virtual bool InitializedCompletely => CompletelyAppeared;
        
        public virtual IPromise OnCreated(IScreenContext context)
        {
            RefreshWidgets();
            OnCreated();
            return OnCreatedDelayed();
        }

        private void InvokeWidgets(Action<IUIMediator> func)
        {
            _widgets.Foreach(m =>
            {
                if (m == null || m as MonoBehaviour == null)
                {
                    return;
                }

                if (m is IUIMediator mediator)
                {
                    func.Invoke(mediator);
                }
            });
        }

        public virtual void OnCreated()
        {
            InvokeWidgets(m => m.OnCreated());
        }

        public virtual IPromise OnCreatedDelayed()
        {
            return Promise.Resolved();
        }

        private void RefreshWidgets()
        {
            _container.Inject(this);
        }

        public virtual TWidget GetWidget<TWidget>() where TWidget : IWidget
        {
            return (TWidget)_widgets?.FirstOrDefault(w => w is TWidget);
        }

        protected TWidget[] GetWidgets<TWidget>() where TWidget : IWidget
        {
            return _widgets?.Where(w => w is TWidget).Cast<TWidget>().ToArray();
        }

        protected List<IWidget> GetAllWidgets()
        {
            return _widgets;
        }

        public virtual void OnDispose()
        {
            InvokeWidgets(w => w.OnDispose());
        }
        
        public FPromisedSignal Shown { get; } = new FPromisedSignal();
        
        public FSignal BeginShow { get; } = new FSignal();
        public FSignal EndShow { get; } = new FSignal();
        
        public FSignal PrepareHide { get; } = new FSignal();
        public FSignal BeginHide { get; } = new FSignal();
        public FSignal EndHide { get; } = new FSignal();

        protected virtual string OnShowSoundKey => string.Empty;
        protected virtual string OnHideSoundKey => string.Empty;
        
        public virtual Vector2 DefaultCurrencyDestinationPosition => Vector2.zero;
        
        public virtual void OnBeginShow()
        {
            if (!OnShowSoundKey.IsNullOrEmpty())
            {
                _soundHandler.PlaySound(OnShowSoundKey);
            }
            
            Shown.Dispatch();
            BeginShow.Dispatch();
            InvokeWidgets(w => w.OnBeginShow());
        }

        public virtual void OnEndShow()
        {
            CompletelyAppeared = true;
            EndShow.Dispatch();
            InvokeWidgets(w => w.OnEndShow());
        }

        public virtual void OnPrepareHide()
        {
            Shown.ResetResult();
            PrepareHide.Dispatch();
        }

        public virtual void OnBeginHide()
        {
            CompletelyAppeared = false;
            
            if (!OnHideSoundKey.IsNullOrEmpty())
            {
                _soundHandler.PlaySound(OnHideSoundKey);
            }
            
            BeginHide.Dispatch();
            InvokeWidgets(w => w.OnBeginHide());
        }

        public virtual void OnEndHide()
        {
            EndHide.Dispatch();
            InvokeWidgets(w => w.OnEndHide());
        }

        public virtual void OnAppearProgress(float progress)
        {
            InvokeWidgets(w => w.OnAppearProgress(progress));
        }

        public virtual void OnDisappearProgress(float progress)
        {
            InvokeWidgets(w => w.OnDisappearProgress(progress));
        }

        public virtual void UpdateScreen()
        {
            
        }

        public virtual void SetRootCanvasGroupVisibility(bool isVisible)
        {
            
        }

        protected void SetRootCanvasGroupVisibility(CanvasGroup rootCanvasGroup, bool isVisible)
        {
            rootCanvasGroup.alpha = isVisible ? 1f : 0f;
            rootCanvasGroup.interactable = isVisible;
            rootCanvasGroup.blocksRaycasts = isVisible;

            if (isVisible && !rootCanvasGroup.gameObject.activeSelf)
            {
                rootCanvasGroup.gameObject.SetActive(true);
            }
        }

        public virtual void RestoreRootCanvas()
        {
        }

        protected void RestoreRootCanvas(CanvasGroup rootCanvasGroup)
        {
            rootCanvasGroup.alpha = 1f;
            rootCanvasGroup.interactable = true;
            rootCanvasGroup.blocksRaycasts = true;
        }
    }
}