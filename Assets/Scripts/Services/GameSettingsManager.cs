using Extensions;
using Handlers;
using Plugins.FSignal;
using Services.Base;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utilities.Disposable;
using Zenject;

namespace Services
{
    public class GameSettingsManager : DisposableService
    {
        private const bool MultiTouchEnabled = true;
        
        [Inject] private readonly GameParamsManager _gameParamsManager;
        [Inject] private readonly LanguageConversionService _languageConversionService;
        [Inject] private readonly SoundHandler _soundHandler;
        
        public FSignal OnLanguageChangedSignal { get; } = new FSignal();
        
        protected override void OnInitialize()
        {
            EnableMultiTouch(MultiTouchEnabled);
            
            _gameParamsManager.GameParamsSnapshotInitializedSignal.MapListener(OnGameParamsSnapshotInitialized).DisposeWith(this);
            _gameParamsManager.MusicChangedSignal.MapListener(OnMusicChangedSignal).DisposeWith(this);
            _gameParamsManager.SoundChangedSignal.MapListener(OnSoundChangedSignal).DisposeWith(this);
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
            if (!_gameParamsManager.IsInitialized)
            {
                LoggerService.LogWarning(this, $"{nameof(InitializeSounds)}: {nameof(GameParamsManager)} is not initialized");
                return;
            }
            
            _soundHandler.SetMusicVolume(_gameParamsManager.IsMusicOn);
            _soundHandler.SetSoundVolume(_gameParamsManager.IsSoundOn);
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
            SetCurrentLanguage(_languageConversionService.GetLocale(_gameParamsManager.LanguageCode));
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

        private void EnableMultiTouch(bool enable)
        {
            Input.multiTouchEnabled = enable;
        }
    }
}