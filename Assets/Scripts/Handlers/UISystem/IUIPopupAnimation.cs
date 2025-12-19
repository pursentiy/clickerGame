using RSG;

namespace Handlers.UISystem
{
    public interface IUIPopupAnimation
    {
        void SetShowedState();
        void SetHiddenState();
        IPromise AnimateShow(object context);
        IPromise AnimateHide(object context);
    }
}