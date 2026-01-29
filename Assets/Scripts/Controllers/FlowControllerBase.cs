using System;
using Extensions;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using RSG;
using Services.Base;
using Utilities.Disposable;

namespace Controllers
{
    public abstract class FlowControllerBase : DisposableService
    {
        protected MediatorFlowInfo ToFlowInfo<T>(IPromise<T> showPromise) where T : class
        {
            var loadPromise = new Promise();
            var hidePromise = new Promise();

            showPromise
                .Then(uiElement => 
                {
                    if (uiElement == null)
                    {
                        throw new NullReferenceException($"UI Element of type {typeof(T)} is null");
                    }

                    TrySubscribeToHide(uiElement, () => hidePromise.SafeResolve());

                    loadPromise.SafeResolve();
                })
                .Catch(e => 
                {
                    loadPromise.SafeReject(e);
                    hidePromise.SafeReject(e);
                })
                .CancelWith(this);

            return new MediatorFlowInfo(loadPromise, hidePromise);
        }

        private void TrySubscribeToHide(object uiElement, Action onHide)
        {
            if (uiElement is UIScreenBase screen)
            {
                screen.EndHide.AddOnce(onHide);
            }
            else if (uiElement is UIPopupBase popup)
            {
                popup.SubscribeOnHide(onHide);
            }
        }
    }
}