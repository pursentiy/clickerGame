using Common.Currency;
using Common.Data.Info;
using Utilities.StateMachine;

namespace Level.FinishLevelSequence
{
    public class FinishLevelContext : IStateContext
    {
        public FinishLevelContext(Stars earnedStars, float levelCompletingTime, PackInfo packInfo)
        {
            EarnedStars = earnedStars;
            LevelCompletingTime = levelCompletingTime;
            PackInfo = packInfo;
        }

        public Stars EarnedStars { get; }
        public float LevelCompletingTime { get; }
        public PackInfo PackInfo { get; }
    }
}