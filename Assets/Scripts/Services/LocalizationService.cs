using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services
{
    public class LocalizationService
    {
        private const string TableKey = "LocalizationTableCommon";
        
        public bool IsInitialized { get; private set; }
        
        private readonly Dictionary<string, Dictionary<string, string>> _tablesCache = new ();
        
        public IEnumerator InitializeRoutine()
        {
            if (IsInitialized)
                yield break;

            LoggerService.LogDebugEditor("Starting Localization Init...");

            var initOp = LocalizationSettings.InitializationOperation;
            while (!initOp.IsDone) 
                yield return null; 
            
            if (initOp.Status == AsyncOperationStatus.Failed)
            {
                LoggerService.LogError($"Localization Init Failed: {initOp.OperationException}");
                yield break;
            }

            var commonTableOp = LocalizationSettings.StringDatabase.GetTableAsync(TableKey);
            while (!commonTableOp.IsDone) 
                yield return null;
            
            if (commonTableOp.Status == AsyncOperationStatus.Succeeded)
            {
                string currentLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
                WarmUpCache(currentLocaleCode, commonTableOp.Result);

                IsInitialized = true;
                LoggerService.LogDebugEditor("Localization Tables preloaded and cached successfully.");
            }
            else
            {
                LoggerService.LogWarning("Failed to preload one or more localization tables.");
            }
        }
        
        // Остальные методы (GetFormatted...) используют GetInternalValue, поэтому тоже ускорятся
        public string GetFormattedValue(string key, params object[] args) 
            => GetFormattedInternalValue(key, TableKey, args);
        
        // Метод для очистки при смене языка, если вы не используете SoftRestart
        public void ClearCache()
        {
            _tablesCache.Clear();
        }

        private void WarmUpCache(string localeCode, StringTable table)
        {
            if (!_tablesCache.ContainsKey(localeCode))
                _tablesCache[localeCode] = new Dictionary<string, string>();

            var cache = _tablesCache[localeCode];
            cache.Clear();

            foreach (var entry in table.SharedData.Entries)
            {
                var localizedString = table.GetEntry(entry.Id)?.LocalizedValue;
                if (localizedString != null)
                {
                    cache[entry.Key] = TmpExtensions.RemoveDiacritics(localizedString);
                }
            }
        }

        public string GetValue(string key) => GetInternalValue(key, TableKey);

        private string GetInternalValue(string key, string tableName)
        {
            if (string.IsNullOrEmpty(key)) return "KEY_EMPTY";
            
            var currentLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;

            if (_tablesCache.TryGetValue(currentLocaleCode, out var table) && table.TryGetValue(key, out var value))
            {
                return value;
            }
            
            return TmpExtensions.RemoveDiacritics(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key));
        }

        private string GetFormattedInternalValue(string key, string tableName, params object[] args)
        {
            var pattern = GetInternalValue(key, tableName);
            if (pattern == "KEY_EMPTY") return pattern;

            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                LoggerService.LogError($"[LocalizationService] Error formatting key: {key} in {tableName}.");
                return pattern;
            }
        }
    }
}