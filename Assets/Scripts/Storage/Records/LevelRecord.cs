using System;
using Common.Currency;

namespace Storage.Records
{
    [Serializable]
    public class LevelRecord
    {
        public LevelRecord(int levelNumber, float bestCompletedTime, int starsEarned, UnlockStatus isUnlocked, int playCount)
        {
            LevelNumber = levelNumber;
            BestCompletedTime = bestCompletedTime;
            StarsEarned = starsEarned;
            IsUnlocked = isUnlocked;
            PlayCount = playCount;
        }
        
        public int LevelNumber;
        public int StarsEarned;
        public float BestCompletedTime;
        public int Score;
        public UnlockStatus IsUnlocked;
        public int PlayCount;
    }
}