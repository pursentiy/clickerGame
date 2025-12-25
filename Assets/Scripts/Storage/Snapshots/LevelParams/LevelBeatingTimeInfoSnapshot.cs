namespace Storage.Snapshots.LevelParams
{
    public class LevelBeatingTimeInfoSnapshot
    {
        public LevelBeatingTimeInfoSnapshot(float fastestTime, float mediumTime, float minimumTime)
        {
            FastestTime = fastestTime;
            MediumTime = mediumTime;
            MinimumTime = minimumTime;
        }

        public float FastestTime { get; }
        public float MediumTime { get; }
        public float MinimumTime { get; }
    }
}