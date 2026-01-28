using Extensions;
using Handlers;
using Plugins.FSignal;
using Services.Base;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utilities.Disposable;
using Zenject;

namespace Services
{
    //TODO MOVE LANGUAGE OUT OF HERE
    public class GameSoundManager : DisposableService
    {
        [Inject] private readonly UserSettingsService _userSettingsService;
        [Inject] private readonly LanguageConversionService _languageConversionService;
        [Inject] private readonly SoundHandler _soundHandler;
        
        public FSignal OnLanguageChangedSignal { get; } = new FSignal();
        
        protected override void OnInitialize()
        {
            _userSettingsService.GameParamsSnapshotInitializedSignal.MapListener(OnGameParamsSnapshotInitialized).DisposeWith(this);
            _userSettingsService.MusicChangedSignal.MapListener(OnMusicChangedSignal).DisposeWith(this);
            _userSettingsService.SoundChangedSignal.MapListener(OnSoundChangedSignal).DisposeWith(this);
        }
        
        protected override void OnDisposing()
        {
            
        }
        
        private void OnGameParamsSnapshotInitialized()
        {
            InitializeSounds();
            InitializeLanguage();
        }

        private void InitializeSounds()
        {
            if (!_userSettingsService.IsInitialized)
            {
                LoggerService.LogWarning(this, $"{nameof(InitializeSounds)}: {nameof(UserSettingsService)} is not initialized");
                return;
            }
            
            _soundHandler.SetMusicVolume(_userSettingsService.IsMusicOn);
            _soundHandler.SetSoundVolume(_userSettingsService.IsSoundOn);
            if (_userSettingsService.IsMusicOn)
                _soundHandler.StartAmbience();
        }
        
        private void OnMusicChangedSignal(bool isOn)
        {
            _soundHandler.SetMusicVolume(isOn);
        }
        
        private void OnSoundChangedSignal(bool isOn)
        {
            _soundHandler.SetSoundVolume(isOn);
        }

        private void InitializeLanguage()
        {
            SetCurrentLanguage(_languageConversionService.GetLocale(_userSettingsService.LanguageCode));
        }
        
        private void SetCurrentLanguage(Locale locale)
        {
            if (locale == null)
            {
                LoggerService.LogError(this, $"{nameof(SetCurrentLanguage)}: trying to set null locale");
                return;
            }
            var localeChanged = LocalizationSettings.SelectedLocale.Identifier.Code != locale.Identifier.Code;
            LocalizationSettings.SelectedLocale = locale;
            
            if (localeChanged)
                OnLanguageChangedSignal.Dispatch();
        }
    }
}