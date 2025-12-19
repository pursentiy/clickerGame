using RSG;

namespace Handlers.UISystem
{
    public interface IUIMediator
    {
        void OnCreated();
        IPromise OnCreatedDelayed();
        void OnBeginShow();
        void OnEndShow();
        void OnPrepareHide();
        void OnBeginHide();
        void OnEndHide();
        void OnAppearProgress(float progress);
        void OnDisappearProgress(float progress);
        void OnDispose();
    }
}