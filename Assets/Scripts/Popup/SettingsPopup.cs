using Handlers;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Zenject;

namespace Popup
{
    public class SettingsPopup : PopupBase
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _soundToggle;

        [SerializeField] private Button _leftLanguageButton;
        [SerializeField] private Button _rightLanguageButton;
        [SerializeField] private Image _countryFlagImage;
        [SerializeField] private TMPro.TextMeshProUGUI _languageLabel;
        
        [SerializeField] private Sprite[] _languageFlags;
        private int _currentLanguageIndex = 0;

        protected override void OnCreated()
        {
            base.OnCreated();

            SetupMusicAndSoundToggles();
            
            _musicToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _progressHandler.ProfileSettingsMusic = isOn;
                _soundHandler.SetMusicVolume(isOn);
            });
            
            _soundToggle.onValueChanged.AddListener(isOn =>
            {
                _soundHandler.PlayButtonSound();
                _progressHandler.ProfileSettingsSound = isOn;
                _soundHandler.SetSoundVolume(isOn);
            });

            _closeButton.onClick.AddListener(() =>
            {
                _soundHandler.PlayButtonSound();
                _popupHandler.HideCurrentPopup();
            });
            
            _leftLanguageButton.onClick.AddListener(() =>
            {
                _soundHandler.PlayButtonSound();
                ChangeLanguage(-1);
            });

            _rightLanguageButton.onClick.AddListener(() =>
            {
                _soundHandler.PlayButtonSound();
                ChangeLanguage(1);
            });

            UpdateLanguagePopup();
        }

        private void SetupMusicAndSoundToggles()
        {
            _musicToggle.isOn = _progressHandler.ProfileSettingsMusic;
            _soundToggle.isOn = _progressHandler.ProfileSettingsSound;
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
            _countryFlagImage.sprite = _languageFlags[_currentLanguageIndex];
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_currentLanguageIndex];
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _musicToggle.onValueChanged.RemoveAllListeners();
            _soundToggle.onValueChanged.RemoveAllListeners();
        }
    }
}