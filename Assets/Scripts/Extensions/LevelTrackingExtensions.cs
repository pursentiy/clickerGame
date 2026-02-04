using Storage.Snapshots.LevelParams;

namespace Extensions
{
    public static class LevelTrackingExtensions
    {
        public static string GetLevelTrackingId(int packId, int levelId)
        {
            return $"{packId}-{levelId}";
        }
    }
}