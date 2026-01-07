using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Services
{
    public class LocalizationService
    {
        private const string CommonTable = "LocalizationTableCommon"; // Общая локаль для проекта
        private const string GameTable = "LocalizationTableGame"; // Для конкретной игры

        public string GetCommonValue(string key) => GetInternalValue(key, CommonTable);

        public string GetFormattedCommonValue(string key, params object[] args) => GetFormattedInternalValue(key, CommonTable, args);

        public string GetGameValue(string key) => GetInternalValue(key, GameTable);

        public string GetFormattedGameValue(string key, params object[] args) => GetFormattedInternalValue(key, GameTable, args);

        private string GetInternalValue(string key, string tableName)
        {
            return string.IsNullOrEmpty(key) ? "KEY_EMPTY" : LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        }

        private string GetFormattedInternalValue(string key, string tableName, params object[] args)
        {
            var pattern = GetInternalValue(key, tableName);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                Debug.LogError(
                    $"[LocalizationService] Error formatting key: {key} in {tableName}. Check placeholders like {{0}}.");
                return pattern;
            }
        }
    }
}