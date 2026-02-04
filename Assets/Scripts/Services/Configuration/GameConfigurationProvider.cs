using System;
using System.Collections.Generic;
using Configurations;
using Services.Base;
using UnityEngine;

namespace Services.Configuration
{
    public class GameConfigurationProvider : DisposableService
    {
        // Кэш для хранения разных типов конфигов
        private readonly Dictionary<Type, ICSVConfig> _configsCache = new();

        public T GetConfig<T>() where T : class, ICSVConfig, new()
        {
            var type = typeof(T);

            if (_configsCache.TryGetValue(type, out var cachedConfig))
            {
                return cachedConfig as T;
            }

            // Пытаемся загрузить файл из ресурсов по имени класса (например, "ProgressConfiguration")
            var configName = type.Name;
            var csvFile = Resources.Load<TextAsset>(configName);

            if (csvFile == null)
            {
                LoggerService.LogError(this, $"[{nameof(GameConfigurationProvider)}] Could not retrieve file Resources/{configName}");
                return null;
            }

            try
            {
                var config = new T();
                config.Parse(csvFile.text);
                
                _configsCache.Add(type, config);
                return config;
            }
            catch (Exception e)
            {
                LoggerService.LogError(this, $"[{nameof(GameConfigurationProvider)}] Parsing Eror {configName}: {e.Message}");
                return null;
            }
        }

        protected override void OnInitialize() { }
        protected override void OnDisposing() => _configsCache.Clear();
    }
}