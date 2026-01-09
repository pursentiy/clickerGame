using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services
{
    public class LocalizationService
    {
        private const string CommonTable = "LocalizationTableCommon";
        private const string GameTable = "LocalizationTableGame";
        
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// ВАЖНО: Вызовите этот метод один раз при старте игры (например, в загрузочном экране)
        /// </summary>
        public IEnumerator InitializeRoutine()
        {
            if (IsInitialized)
            {
                yield break;
            }

            LoggerService.LogDebugEditor("Starting Localization Init...");
            
            var initOp = LocalizationSettings.InitializationOperation;
        
            while (!initOp.IsDone)
            {
                yield return null; 
            }

            if (initOp.Status == AsyncOperationStatus.Failed)
            {
                LoggerService.LogError($"Localization Init Failed: {initOp.OperationException}");
                yield break;
            }

            LoggerService.LogDebugEditor("Localization Settings Initialized. Loading Tables...");
            
            var commonTableOp = LocalizationSettings.StringDatabase.GetTableAsync("LocalizationTableCommon");
            var gameTableOp = LocalizationSettings.StringDatabase.GetTableAsync("LocalizationTableGame");
            
            while (!commonTableOp.IsDone || !gameTableOp.IsDone)
            {
                yield return null;
            }

            if (commonTableOp.Status == AsyncOperationStatus.Succeeded && gameTableOp.Status == AsyncOperationStatus.Succeeded)
            {
                IsInitialized = true;
                LoggerService.LogDebugEditor("Localization Tables preloaded successfully.");
            }
            else
            {
                LoggerService.LogWarning("Failed to preload one or more localization tables.");
            }
        }

        public string GetCommonValue(string key) => GetInternalValue(key, CommonTable);

        public string GetGameValue(string key) => GetInternalValue(key, GameTable);

        public string GetFormattedCommonValue(string key, params object[] args) 
            => GetFormattedInternalValue(key, CommonTable, args);

        public string GetFormattedGameValue(string key, params object[] args) 
            => GetFormattedInternalValue(key, GameTable, args);

        private string GetInternalValue(string key, string tableName)
        {
            if (string.IsNullOrEmpty(key)) return "KEY_EMPTY";

            if (!IsInitialized)
            {
                LoggerService.LogWarning($"[LocalizationService] Accessing key '{key}' before initialization!");
            }

            return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
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