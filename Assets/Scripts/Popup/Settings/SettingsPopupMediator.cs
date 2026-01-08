using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Services;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Zenject;

namespace Popup.Settings
{
    [AssetKey("UI Popups/SettingsPopupMediator")]
    public class SettingsPopupMediator : UIPopupBase<SettingsPopupView>
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private GlobalSettingsService _globalSettingsService;
        
        private int _currentLanguageIndex = 0;
        
        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            SetupMusicAndSoundToggles();
            
            View.MusicToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _globalSettingsService.ProfileSettingsMusic = isOn;
                _soundHandler.SetMusicVolume(isOn);
            });
            
            View.SoundToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _globalSettingsService.ProfileSettingsSound = isOn;
                _soundHandler.SetSoundVolume(isOn);
            });

            View.CloseButton.onClick.MapListenerWithSound(Hide);
            View.LeftLanguageButton.onClick.MapListenerWithSound(() => ChangeLanguage(-1));
            View.RightLanguageButton.onClick.MapListenerWithSound(() => ChangeLanguage(1));

            UpdateLanguagePopup();
        }

        private void SetupMusicAndSoundToggles()
        {
            View.MusicToggle.isOn = _globalSettingsService.ProfileSettingsMusic;
            View.SoundToggle.isOn = _globalSettingsService.ProfileSettingsSound;
        }
        
        private void ChangeLanguage(int direction)
        {
            _currentLanguageIndex += direction;

            if (_currentLanguageIndex < 0)
                _currentLanguageIndex = LocalizationSettings.AvailableLocales.Locales.Count - 1;
            else if (_currentLanguageIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
                _currentLanguageIndex = 0;

            UpdateLanguagePopup();
            PlayerPrefs.SetInt("language_index", _currentLanguageIndex);
        }

        private void UpdateLanguagePopup()
        {
            View.CountryFlagImage.sprite = View.LanguageFlags[_currentLanguageIndex];
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_currentLanguageIndex];
        }

        private void OnDestroy()
        {
            View.MusicToggle.onValueChanged.RemoveAllListeners();
            View.SoundToggle.onValueChanged.RemoveAllListeners();
        }
    }
}