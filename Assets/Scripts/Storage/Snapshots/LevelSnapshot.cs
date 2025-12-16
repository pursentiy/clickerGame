namespace Storage.Snapshots
{
    public class LevelSnapshot
    {
        public LevelSnapshot(int levelNumber, float levelCompletedTime, int starsEarned)
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