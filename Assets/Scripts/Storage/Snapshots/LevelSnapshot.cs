using Common.Currency;

namespace Storage.Snapshots
{
    public class LevelSnapshot
    {
        public LevelSnapshot(int levelId, float bestCompletedTime, Stars starsEarned, UnlockStatus isUnlocked, int playCount)
        {
            LevelId = levelId;
            BestCompletedTime = bestCompletedTime;
            StarsEarned = starsEarned;
            IsUnlocked = isUnlocked;
            PlayCount = playCount;
        }

        public int LevelId {get; set;}
        public float BestCompletedTime {get; set;}
        public Stars StarsEarned {get; set;}
        public UnlockStatus IsUnlocked {get; set;}
        public int PlayCount {get; set;}
    }
}