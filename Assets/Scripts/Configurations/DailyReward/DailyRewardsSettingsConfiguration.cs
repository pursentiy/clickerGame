namespace Configurations.DailyReward
{
    /// <summary>
    /// Static settings for daily rewards (cycle length, default interval).
    /// </summary>
    public static class DailyRewardsSettingsConfiguration
    {
        public const int CycleLength = 6;
        public const int DefaultRewardIntervalMinutes = 2;
        public const float GracePeriodMultiplier = 2.0f;
    }
}
