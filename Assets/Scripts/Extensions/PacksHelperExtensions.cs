using Configurations.Progress;
using Services;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;

namespace Extensions
{
    public static class PacksHelperExtensions
    {
        public static bool IsAvailable(this PackStatus status) => status == PackStatus.Available;
        public static bool IsLocked(this PackStatus status) => status == PackStatus.Locked;
        public static bool CanBeUnlocked(this PackStatus status) => status == PackStatus.CanBeUnlocked;
        public static bool IsUnavailablePack(this PackStatus status) => status == PackStatus.UnavailablePack;

        /// <summary>Safe cast from <see cref="IPackClickAction"/> to the desired concrete type (e.g. <see cref="PackClickAction"/>). Returns null if the instance is not of type <typeparamref name="T"/>; logs error via <see cref="LoggerService"/> when null.</summary>
        public static T As<T>(this IPackClickAction action) where T : class, IPackClickAction
        {
            if (action == null)
            {
                LoggerService.LogError($"[{nameof(PacksHelperExtensions)}] {nameof(As)}<{typeof(T).Name}> failed: {nameof(IPackClickAction)} is null.");
                return null;
            }
            var result = action as T;
            if (result == null)
                LoggerService.LogError($"[{nameof(PacksHelperExtensions)}] {nameof(As)}<{typeof(T).Name}> failed: {nameof(IPackClickAction)} is not of type {typeof(T).Name}.");
            return result;
        }

        /// <summary>Safe cast from <see cref="IPackClickAction"/> to <see cref="PackClickAction"/>. Returns null if the instance is not a <see cref="PackClickAction"/>; logs error via <see cref="LoggerService"/> when null.</summary>
        public static PackClickAction AsPackClickAction(this IPackClickAction action)
        {
            if (action == null)
            {
                LoggerService.LogError($"[{nameof(PacksHelperExtensions)}] {nameof(AsPackClickAction)} failed: {nameof(IPackClickAction)} is null.");
                return null;
            }
            var result = action as PackClickAction;
            if (result == null)
                LoggerService.LogError($"[{nameof(PacksHelperExtensions)}] {nameof(AsPackClickAction)} failed: {nameof(IPackClickAction)} is not of type {nameof(PackClickAction)}.");
            return result;
        }
    }
}