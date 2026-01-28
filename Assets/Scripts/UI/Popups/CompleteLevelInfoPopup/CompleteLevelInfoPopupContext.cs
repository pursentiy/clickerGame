using Common.Currency;
using Handlers.UISystem.Popups;

namespace UI.Popups.CompleteLevelInfoPopup
{
    public class CompleteLevelInfoPopupContext : IPopupContext
    {
        public CompleteLevelInfoPopupContext(
            Stars currentStars,
            Stars initialStars,
            Stars preRewardBalance,
            float beatTime, 
            CompletedLevelStatus levelStatus)
        {

            CurrentStars = currentStars;
            InitialStars = initialStars;
            PreRewardBalance = preRewardBalance;
            BeatTime = beatTime;
            LevelStatus = levelStatus;
        }
        
        public Stars CurrentStars { get; }
        public Stars InitialStars { get; }
        public Stars PreRewardBalance { get; }
        public float BeatTime { get; }
        public CompletedLevelStatus LevelStatus { get; }
    }

    public enum CompletedLevelStatus
    {
        InitialCompletion,
        Replayed
    }
}