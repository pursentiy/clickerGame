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
        [Inject] private PlayerLevelService _playerLevelService;
        [Inject] private CheatService _cheatService;
        
        [SerializeField] private Sprite[] _languageFlags;
        private int _currentLanguageIndex = 0;
        
        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            SetupMusicAndSoundToggles();
            
            View.MusicToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _playerLevelService.ProfileSettingsMusic = isOn;
                _soundHandler.SetMusicVolume(isOn);
            });
            
            View.SoundToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _playerLevelService.ProfileSettingsSound = isOn;
                _soundHandler.SetSoundVolume(isOn);
            });

            View.CloseButton.onClick.MapListenerWithSound(Hide);
            View.LeftLanguageButton.onClick.MapListenerWithSound(() => ChangeLanguage(-1));
            View.RightLanguageButton.onClick.MapListenerWithSound(() => ChangeLanguage(1));

            //TODO CHEAT REMOVE
            View.ResetProgressButton.onClick.MapListenerWithSound(CheatResetProgress);

            UpdateLanguagePopup();
        }

        private void SetupMusicAndSoundToggles()
        {
            View.MusicToggle.isOn = _playerLevelService.ProfileSettingsMusic;
            View.SoundToggle.isOn = _playerLevelService.ProfileSettingsSound;
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
            View.CountryFlagImage.sprite = _languageFlags[_currentLanguageIndex];
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_currentLanguageIndex];
        }

        private void CheatResetProgress()
        {
            _cheatService.CheatResetProgress();
        }

        private void OnDestroy()
        {
            View.MusicToggle.onValueChanged.RemoveAllListeners();
            View.SoundToggle.onValueChanged.RemoveAllListeners();
        }
    }
}