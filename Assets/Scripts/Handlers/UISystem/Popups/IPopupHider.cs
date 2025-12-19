using RSG;

namespace Handlers.UISystem.Popups
{
    public interface IPopupHider
    {
        void HidePopup(UIPopupBase popup);
        Promise HidePopupWithDelayedDispose(UIPopupBase popup);
    }
}