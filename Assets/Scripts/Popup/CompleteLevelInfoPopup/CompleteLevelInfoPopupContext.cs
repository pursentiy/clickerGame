using System;
using Handlers.UISystem.Popups;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            int totalStars,
            int earnedStars,
            float totalTime,
            Action goToMenuAction)
        {
            TotalStars = totalStars;
            EarnedStars = earnedStars;
            TotalTime = totalTime;
            GoToMenuAction = goToMenuAction;
        }

        public int TotalStars { get; }
        public int EarnedStars { get; }
        public float TotalTime { get; }
        public Action GoToMenuAction { get; }
    }
}