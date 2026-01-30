using System;
using Handlers.UISystem;
using Handlers.UISystem.Popups;
using RSG;
using UI.Popups.MessagePopup;
using UI.Popups.SettingsPopup;
using UnityEngine;
using Utilities;
using Utilities.Disposable;
using Zenject;

namespace Controllers
{
    public sealed class FlowPopupController : FlowControllerBase
    {
        [Inject] private readonly UIManager _uiManager;
        
        public MediatorFlowInfo ShowSettingsPopup(bool allowLanguageChanging,  PopupShowingOptions option = PopupShowingOptions.Immediately, IDisposeProvider overrideDisposeProvider = null)
        {
            var popupPromise = ShowPopupInternally<SettingsPopupMediator>(new SettingsPopupContext(allowLanguageChanging), option, overrideDisposeProvider);
            return ToFlowInfo<SettingsPopupMediator>(popupPromise);
        }
        
        public MediatorFlowInfo ShowMessagePopup(MessagePopupContext context,  PopupShowingOptions option = PopupShowingOptions.Immediately, IDisposeProvider overrideDisposeProvider = null)
        {
            var popupPromise = ShowPopupInternally<MessagePopupMediator>(context, option, overrideDisposeProvider);
            return ToFlowInfo<MessagePopupMediator>(popupPromise);
        }
        
        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            
        }
        
        private IPromise<TPopup> ShowPopupInternally<TPopup>(IPopupContext context, PopupShowingOptions option, IDisposeProvider overrideDisposeProvider = null) where TPopup : UIPopupBase
        {
            var disposeProvider = overrideDisposeProvider ?? this;
            return option switch
            {
                PopupShowingOptions.Immediately => _uiManager.PopupsHandler.ShowPopupImmediately<TPopup>(context).CancelWith(disposeProvider),
                PopupShowingOptions.Enqueue => _uiManager.PopupsHandler.EnqueuePopup<TPopup>(context).CancelWith(disposeProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }
    }

    public enum PopupShowingOptions
    {
        Immediately,
        Enqueue,
    }
}