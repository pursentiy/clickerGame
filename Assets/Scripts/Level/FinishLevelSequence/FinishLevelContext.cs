using Common.Currency;
using Common.Data.Info;
using UI.Popups.CompleteLevelInfoPopup;
using Utilities.StateMachine;

namespace Level.FinishLevelSequence
{
    public class FinishLevelContext : IStateContext
    {
        public FinishLevelContext(
            Stars earnedStars,
            Stars previousStarsForLevel,
            float levelCompletingTime,
            PackInfo packInfo,
            CompletedLevelStatus completedLevelStatus)
        {
            EarnedStars = earnedStars;
            PreviousStarsForLevel = previousStarsForLevel;
            LevelCompletingTime = levelCompletingTime;
            PackInfo = packInfo;
            CompletedLevelStatus = completedLevelStatus;
        }
        
        public Stars EarnedStars { get; }
        public Stars PreviousStarsForLevel { get; }
        public float LevelCompletingTime { get; }
        public PackInfo PackInfo { get; }
        public CompletedLevelStatus CompletedLevelStatus { get; }
    }
}