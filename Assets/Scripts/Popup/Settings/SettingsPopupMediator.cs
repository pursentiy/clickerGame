using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Popup.Universal;
using Services;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Utilities.Disposable;
using Zenject;

namespace Popup.Settings
{
    [AssetKey("UI Popups/SettingsPopupMediator")]
    public class SettingsPopupMediator : UIPopupBase<SettingsPopupView>
    {
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly GlobalSettingsService _globalSettingsService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ReloadService _reloadService;

        private int _currentLanguageIndex;
        private int _pendingLanguageIndex;
        
        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();
            
            _currentLanguageIndex = _pendingLanguageIndex = PlayerPrefs.GetInt("language_index", 0);
            UpdateLocalizationSettings(_currentLanguageIndex);
            UpdateLanguageSprite(_currentLanguageIndex);
            SetupMusicAndSoundToggles();
            
            View.MusicToggle.onValueChanged.MapListenerWithSound(isOn =>
            {
                _globalSettingsService.ProfileSettingsMusic = isOn;
                _soundHandler.SetMusicVolume(isOn);
            }).DisposeWith(this);
            
            View.SoundToggle.onValueChanged.MapListenerWithSound(isOn =>
            {
                _globalSettingsService.ProfileSettingsSound = isOn;
                _soundHandler.SetSoundVolume(isOn);
            }).DisposeWith(this);

            View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.SaveLanguageButton.onClick.MapListenerWithSound(SaveLanguage).DisposeWith(this);
            View.LeftLanguageButton.onClick.MapListenerWithSound(() => SetPendingLanguage(-1)).DisposeWith(this);
            View.RightLanguageButton.onClick.MapListenerWithSound(() => SetPendingLanguage(1)).DisposeWith(this);
        }

        private void SetupMusicAndSoundToggles()
        {
            View.MusicToggle.isOn = _globalSettingsService.ProfileSettingsMusic;
            View.SoundToggle.isOn = _globalSettingsService.ProfileSettingsSound;
        }
        
        private void SetPendingLanguage(int direction)
        {
            var newLanguageIndex = GetNewLanguageIndex(direction);
            
            if (newLanguageIndex < 0 || newLanguageIndex >= View.LanguageFlags.Length)
                return;

            _pendingLanguageIndex = newLanguageIndex;
            UpdateLanguageSprite(_pendingLanguageIndex);
        }

        private int GetNewLanguageIndex(int direction)
        {
            var newLanguageIndex = _pendingLanguageIndex;
            newLanguageIndex += direction;

            var localesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            if (newLanguageIndex < 0)
                newLanguageIndex = localesCount - 1;
            else if (newLanguageIndex >= localesCount)
                newLanguageIndex = 0;

            return newLanguageIndex;
        }
        
        private void ShowConfirmLanguagePopup()
        {
            var spriteAsset = View.LanguageFlagsAssets[_pendingLanguageIndex];
            
            //TODO LOCALIZATION
            var reloadButton = new UniversalPopupButtonAction("Релоад", ApplyLanguageChanges);
            var cancelButton = new UniversalPopupButtonAction("Отмена", null, UniversalPopupButtonStyle.Red);
            var context = new UniversalPopupContext(
                "Если вы поменяете язык, то игра перезагрузиться. Вы точно хотите поменять язык на <sprite=0> ?",
                new[] { cancelButton, reloadButton }, "Смена языка", spriteAsset: spriteAsset);
            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);

            void ApplyLanguageChanges()
            {
                SaveNewLanguageIndex(_pendingLanguageIndex);
                _reloadService.SoftRestart();
            }
        }

        private void SaveNewLanguageIndex(int newLanguageIndex)
        {
            _currentLanguageIndex = newLanguageIndex;
            UpdateLocalizationSettings(_currentLanguageIndex);
            UpdateLanguageSprite(_currentLanguageIndex);
            
            PlayerPrefs.SetInt("language_index", _currentLanguageIndex);
        }

        private void SaveLanguage()
        {
            if (_pendingLanguageIndex == _currentLanguageIndex)
                return;
            
            ShowConfirmLanguagePopup();
        }

        private void UpdateLanguageSprite(int languageIndex)
        {
            if (View.LanguageFlags.IsNullOrEmpty() 
                || languageIndex < 0 
                || languageIndex >= View.LanguageFlags.Length) 
                return;
            
            View.CountryFlagImage.sprite = View.LanguageFlags[languageIndex];
        }
        
        private void UpdateLocalizationSettings(int languageIndex)
        {
            if (LocalizationSettings.AvailableLocales.Locales.IsNullOrEmpty() 
                || languageIndex < 0 
                || languageIndex >= LocalizationSettings.AvailableLocales.Locales.Count) 
                return;
            
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
        }
    }
}