using RSG;

namespace Handlers
{
    public interface IPopupHandler
    {
        IPromise HideCurrentPopup();
        void ShowSettings();
    }
}