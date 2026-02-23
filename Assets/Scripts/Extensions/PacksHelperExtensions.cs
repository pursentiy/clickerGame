using Configurations.Progress;

namespace Extensions
{
    public static class PacksHelperExtensions
    {
        public static bool IsAvailable(this PackStatus status) => status == PackStatus.Available;
        public static bool IsLocked(this PackStatus status) => status == PackStatus.Locked;
        public static bool IsCanBeUnlocked(this PackStatus status) => status == PackStatus.CanBeUnlocked;
        public static bool IsUnavailablePack(this PackStatus status) => status == PackStatus.UnavailablePack;
    }
}