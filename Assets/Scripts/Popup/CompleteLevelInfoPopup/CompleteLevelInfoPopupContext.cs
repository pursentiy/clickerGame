using System;
using Handlers.UISystem.Popups;

namespace Popup.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(int totalStars, int earnedStars, float totalTime, Action restartLevelAction)
        {
            TotalStars = totalStars;
            EarnedStars = earnedStars;
            TotalTime = totalTime;
            RestartLevelAction = restartLevelAction;
        }

        public int TotalStars { get; }
        public int EarnedStars { get; }
        public float TotalTime { get; }
        public Action RestartLevelAction { get; }
    }
}