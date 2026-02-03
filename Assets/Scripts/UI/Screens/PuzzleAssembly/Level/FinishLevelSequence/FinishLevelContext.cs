using Common.Currency;
using UI.Popups.CompleteLevelInfoPopup;
using Utilities.StateMachine;

namespace UI.Screens.PuzzleAssembly.Level.FinishLevelSequence
{
    public class FinishLevelContext : IStateContext
    {
        public FinishLevelContext(
            int packId, 
            int levelId,
            Stars currentStars,
            Stars initialStars,
            float levelCompletingTime,
            CompletedLevelStatus completedLevelStatus)
        {
            PackId = packId;
            LevelId = levelId;
            CurrentStars = currentStars;
            InitialStars = initialStars;
            LevelCompletingTime = levelCompletingTime;
            CompletedLevelStatus = completedLevelStatus;
        }
        
        public int PackId { get; }
        public int LevelId { get; }
        public Stars CurrentStars { get; }
        public Stars InitialStars { get; }
        public float LevelCompletingTime { get; }
        public CompletedLevelStatus CompletedLevelStatus { get; }
    }
}