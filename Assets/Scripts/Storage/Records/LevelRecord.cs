namespace Storage.Records
{
    public class LevelRecord
    {
        public LevelRecord(int levelNumber, float levelCompletedTime, int starsEarned)
        {
            LevelNumber = levelNumber;
            LevelCompletedTime = levelCompletedTime;
            StarsEarned = starsEarned;
        }

        public int LevelNumber {get; set;}
        public float LevelCompletedTime {get; set;}
        public int StarsEarned {get; set;}
    }
}