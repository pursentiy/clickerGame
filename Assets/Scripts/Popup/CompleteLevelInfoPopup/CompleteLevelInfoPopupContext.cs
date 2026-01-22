using System;
using Common.Currency;
using Handlers.UISystem.Popups;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            Stars totalStars,
            float totalTime,
            Action goToMenuAction)
        {
            TotalStars = totalStars;
            TotalTime = totalTime;
            GoToMenuAction = goToMenuAction;
        }

        public Stars TotalStars { get; }
        public float TotalTime { get; }
        public Action GoToMenuAction { get; }
    }
}