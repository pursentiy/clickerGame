using Services.Base;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Services
{
    public class GlobalSettingsService : DisposableService
    {
        public void SetCurrentLanguage(Locale locale)
        {
            if (locale == null)
            {
                LoggerService.LogError(this, $"{nameof(SetCurrentLanguage)}: trying to set null locale");
                return;
            }
            
            LocalizationSettings.SelectedLocale = locale;
        }
        
        protected override void OnInitialize()
        {
            Input.multiTouchEnabled = false;
        }

        protected override void OnDisposing()
        {
            
        }
    }
}