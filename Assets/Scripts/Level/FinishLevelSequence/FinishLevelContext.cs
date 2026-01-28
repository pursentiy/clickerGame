using Common.Currency;
using Common.Data.Info;
using UI.Popups.CompleteLevelInfoPopup;
using Utilities.StateMachine;

namespace Level.FinishLevelSequence
{
    public class FinishLevelContext : IStateContext
    {
        public FinishLevelContext(
            int packId, 
            int levelId,
            Stars currentStars,
            Stars initialStars,
            float levelCompletingTime,
            PackInfo packInfo,
            CompletedLevelStatus completedLevelStatus)
        {
            PackId = packId;
            LevelId = levelId;
            CurrentStars = currentStars;
            InitialStars = initialStars;
            LevelCompletingTime = levelCompletingTime;
            PackInfo = packInfo;
            CompletedLevelStatus = completedLevelStatus;
        }
        
        public int PackId { get; }
        public int LevelId { get; }
        public Stars CurrentStars { get; }
        public Stars InitialStars { get; }
        public float LevelCompletingTime { get; }
        public PackInfo PackInfo { get; }
        public CompletedLevelStatus CompletedLevelStatus { get; }
    }
}