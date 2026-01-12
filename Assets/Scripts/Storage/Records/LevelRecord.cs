using System;

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
        public int PlayCount;
        
        //TODO IN THE FUTURE BLOCK
        public UnlockStatus IsUnlocked;
    }
}