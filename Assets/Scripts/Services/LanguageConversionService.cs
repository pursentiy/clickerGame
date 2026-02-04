using System;
using Extensions;
using Playgama;
using Services.Static;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Services
{
    public class LanguageConversionService
    {
        private const string Russian = "ru";
        private const string English = "en";
        private const string Spanish = "es";

        public int AvailableLocalesCount => LocalizationSettings.AvailableLocales.Locales.Count;
        public Locale SelectedLocale => LocalizationSettings.SelectedLocale;
        public string FallbackLanguageCode => English;

        public int GetSelectedLocaleIndex()
        {
            //TODO ADD NRE REFERENCE
            var index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);

            return index < 0 ? 0 : index;
        }

        public string GetLocaleLanguageCodeByIndex(int index)
        {
            if (index < 0 || index >= LocalizationSettings.AvailableLocales.Locales.Count)
            {
                LoggerService.LogError(this, $"{nameof(GetLocaleLanguageCodeByIndex)}: index out of range: {index}");
                return GetLocaleLanguageCode(GetDefaultLocale());
            }
        
            return GetLocaleLanguageCode(LocalizationSettings.AvailableLocales.Locales[index]);
        }

        public Locale GetDefaultLocale()
        {
            return GetLocaleFromCode(GetDefaultLanguageCode());
        }

        public string GetDefaultLanguageCode()
        {
            var platformLang = English;
            try
            {
                if (AppConfigService.IsProduction())
                {
                    platformLang = FromBridgeToInternalCode(Bridge.platform.language);
                }
            
                return platformLang;
            }
            catch (Exception e)
            {
                LoggerService.LogError(this, $"{nameof(GetDefaultLanguageCode)}: exception: {e}");
                return platformLang;
            }
        }

        public Locale GetLocale(string languageCode)
        {
            if (languageCode.IsNullOrEmpty())
                return GetDefaultLocale();

            return GetLocaleFromCode(languageCode);
        }

        public string GetCurrentLocaleLanguageCode()
        {
            if (LocalizationSettings.SelectedLocale == null)
            {
                return English;
            }

            return LocalizationSettings.SelectedLocale.Identifier.Code;
        }
        
        public string GetLocaleLanguageCode(Locale locale)
        {
            if (locale == null)
            {
                LoggerService.LogError(this, $"{nameof(GetLocaleLanguageCode)}: {nameof(Locale)} is null");
                return English;
            }

            return locale.Identifier.Code;
        }
        
        private Locale GetLocaleFromCode(string languageCode)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;

            if (locales.IsNullOrEmpty())
            {
                LoggerService.LogError(this, $"{nameof(LocalizationSettings)} has no available locales");
                return null;
            }
            
            var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            return locale != null ? locale : LocalizationSettings.SelectedLocale;
        }
        
        private string FromBridgeToInternalCode(string bridgeLang)
        {
            if (string.IsNullOrEmpty(bridgeLang)) 
                return English;
            
            bridgeLang = bridgeLang.ToLower().Trim();
            
            if (bridgeLang.StartsWith("ru") || bridgeLang.StartsWith("be") || bridgeLang.StartsWith("kk"))
                return Russian;
            
            if (bridgeLang.StartsWith("es"))
                return Spanish;
            
            if (bridgeLang.StartsWith("en"))
                return English;

            return English;
        }
    }
}