using System;

namespace Storage.Records
{
    [Serializable]
    public class LevelRecord
    {
        public LevelRecord(int levelNumber, float levelCompletedTime, int starsEarned)
        {
            LevelNumber = levelNumber;
            LevelCompletedTime = levelCompletedTime;
            StarsEarned = starsEarned;
        }

        public int LevelNumber;
        public float LevelCompletedTime;
        public int StarsEarned;
    }
}