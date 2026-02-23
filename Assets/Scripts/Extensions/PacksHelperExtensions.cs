using Configurations.Progress;

namespace Extensions
{
    public static class PacksHelperExtensions
    {
        public static bool IsFreePack(this PackUnlockCurrencyStatus status)
        {
            return status == PackUnlockCurrencyStatus.FreePack;
        }
        
        public static bool IsFreemiumPack(this PackUnlockCurrencyStatus status)
        {
            return !IsFreePack(status);
        }

        public static bool EnoughCurrencyToUnlockPack(this PackUnlockCurrencyStatus status)
        {
            return status == PackUnlockCurrencyStatus.AvailableToUnlock;
        }
    }
}