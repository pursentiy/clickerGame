using Common.Currency;
using Handlers.UISystem.Popups;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            Stars totalStars,
            float totalTime)
        {
            TotalStars = totalStars;
            TotalTime = totalTime;
        }

        public Stars TotalStars { get; }
        public float TotalTime { get; }
    }
}