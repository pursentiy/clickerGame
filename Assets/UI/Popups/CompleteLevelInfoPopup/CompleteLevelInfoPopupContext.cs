using Common.Currency;
using Handlers.UISystem.Popups;

namespace UI.Popups.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            Stars earnedStars,
            Stars previousStarsForLevel,
            float totalTime, CompletedLevelStatus completedLevelStatus)
        {
            EarnedStars = earnedStars;
            PreviousStarsForLevel = previousStarsForLevel;
            TotalTime = totalTime;
            CompletedLevelStatus = completedLevelStatus;
        }
        
        public Stars EarnedStars { get; }
        public Stars PreviousStarsForLevel { get; }
        public float TotalTime { get; }
        public CompletedLevelStatus CompletedLevelStatus { get; }
    }

    public enum CompletedLevelStatus
    {
        InitialCompletion,
        Replayed
    }
}