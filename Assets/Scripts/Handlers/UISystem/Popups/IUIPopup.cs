using System;
using Plugins.FSignal;

namespace Handlers.UISystem.Popups
{
    public interface IUIPopup
    {
        int Priority { get; }
        IUIPopup SubscribeOnHide(Action onHide);
        FSignal OnBeginHideSignal { get; }
        void Hide();
        void HideByBackButton();
        void SetHider(IPopupHider hider);
    }
}