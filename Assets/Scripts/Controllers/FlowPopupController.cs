using System;
using Handlers.UISystem;
using Handlers.UISystem.Popups;
using RSG;
using UI.Popups.SettingsPopup;
using Zenject;

namespace Controllers
{
    public sealed class FlowPopupController : FlowControllerBase
    {
        [Inject] private readonly UIManager _uiManager;
        
        public MediatorFlowInfo ShowSettingsPopup(bool allowLanguageChanging,  PopupShowingOptions option = PopupShowingOptions.Immediately)
        {
            var popupPromise = ShowPopupInternally<SettingsPopupMediator>(new SettingsPopupContext(allowLanguageChanging), option);
            return ToFlowInfo<SettingsPopupMediator>(popupPromise);
        }
        
        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            
        }
        
        private IPromise<TPopup> ShowPopupInternally<TPopup>(IPopupContext context, PopupShowingOptions option) where TPopup : UIPopupBase
        {
            return option switch
            {
                PopupShowingOptions.Immediately => _uiManager.PopupsHandler.ShowPopupImmediately<TPopup>(context),
                PopupShowingOptions.Enqueue => _uiManager.PopupsHandler.EnqueuePopup<TPopup>(context),
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