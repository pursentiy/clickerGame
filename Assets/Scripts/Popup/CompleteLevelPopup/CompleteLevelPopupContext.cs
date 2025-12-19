using Handlers.UISystem.Popups;
using Popup.Base;

namespace Popup.CompleteLevelPopup
{
    public class CompleteLevelPopupContext : IPopupContext
    {
        public CompleteLevelPopupContext(int totalStars, int earnedStars, float totalTime)
        {
            TotalStars = totalStars;
            EarnedStars = earnedStars;
            TotalTime = totalTime;
        }

        public int TotalStars { get; }
        public int EarnedStars { get; }
        public float TotalTime { get; }
    }
}