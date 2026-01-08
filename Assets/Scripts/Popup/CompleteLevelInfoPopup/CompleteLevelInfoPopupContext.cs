using System;
using Common.Currency;
using Handlers.UISystem.Popups;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            Stars totalStars,
            Stars earnedStars,
            float totalTime,
            Action goToMenuAction)
        {
            TotalStars = totalStars;
            EarnedStars = earnedStars;
            TotalTime = totalTime;
            GoToMenuAction = goToMenuAction;
        }

        public Stars TotalStars { get; }
        public Stars EarnedStars { get; }
        public float TotalTime { get; }
        public Action GoToMenuAction { get; }
    }
}