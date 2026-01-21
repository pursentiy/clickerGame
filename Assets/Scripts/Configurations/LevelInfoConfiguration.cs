namespace Configurations
{
    public class LevelInfoConfiguration
    {
        public int LevelId { get; private set; }
        public string LevelName { get; private set; }
        public float[] BeatingTimes { get; private set; }
        public float FigureScale { get; private set; }
        public LevelDifficulty Difficulty { get; private set; }

        public LevelInfoConfiguration(int levelId, string levelName, float[] beatingTimes, float figureScale, LevelDifficulty difficulty)
        {
            LevelId = levelId;
            LevelName = levelName;
            BeatingTimes = beatingTimes;
            FigureScale = figureScale;
            Difficulty = difficulty;
        }
    }
}