using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Services;
using Services.Player;
using UI.Popups.CommonPopup;
using UI.Popups.UniversalPopup;
using Utilities.Disposable;
using Zenject;

namespace UI.Popups.SettingsPopup
{
    [AssetKey("UI Popups/SettingsPopupMediator")]
    public class SettingsPopupMediator : UIPopupBase<SettingsPopupView, SettingsPopupContext>
    {
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly GameSoundManager _gameSoundManager;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly ReloadService _reloadService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly PlayerProfileController _playerProfileController;
        [Inject] private readonly LanguageConversionService _languageConversionService;
        [Inject] private readonly UserSettingsService _userSettingsService;

        private int _currentLanguageIndex;
        private int _pendingLanguageIndex;
        private int _localesCount;
    
        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            _localesCount = _languageConversionService.AvailableLocalesCount;
            _currentLanguageIndex = _languageConversionService.GetSelectedLocaleIndex();
            _pendingLanguageIndex = _currentLanguageIndex;
        
            RefreshLanguageUI();
            SetupToggles();
            SetupLanguageSettingsGroup();
        
            View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.BackgroundButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
        }

        private void SetupLanguageSettingsGroup()
        {
            View.LanguageChangingContainer.TrySetActive(Context.AllowLanguageChanging);
            if (Context.AllowLanguageChanging)
            {
                View.LeftLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(-1)).DisposeWith(this);
                View.RightLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(1)).DisposeWith(this);
                View.SaveLanguageButton.onClick.MapListenerWithSound(TrySaveLanguage).DisposeWith(this);
            }
        }

        private void SetupToggles()
        {
            View.MusicToggle.SetIsOnWithoutNotify(_userSettingsService.IsMusicOn);
            View.SoundToggle.SetIsOnWithoutNotify(_userSettingsService.IsSoundOn);

            View.MusicToggle.onValueChanged.MapListenerWithSound(OnMusicToggled).DisposeWith(this);
            View.SoundToggle.onValueChanged.MapListenerWithSound(OnSoundToggled).DisposeWith(this);
        }

        private void OnSoundToggled(bool isOn)
        {
            _userSettingsService.SetSoundAvailable(isOn);
            _soundHandler.SetSoundVolume(isOn);
        }
    
        private void OnMusicToggled(bool isOn)
        {
            _userSettingsService.SetMusicAvailable(isOn);
            _soundHandler.SetMusicVolume(isOn);
        
            if (isOn)
                _soundHandler.StartAmbience();
            else
                _soundHandler.StopAmbience();
        }

        private void ChangePendingIndex(int direction)
        {
            _pendingLanguageIndex = (_pendingLanguageIndex + direction + _localesCount) % _localesCount;
            RefreshLanguageUI();
        }

        private void RefreshLanguageUI()
        {
            View.SaveLanguageButton.TrySetActive(_pendingLanguageIndex != _currentLanguageIndex);
        
            if (View.LanguageFlags.Length > _pendingLanguageIndex)
                View.CountryFlagImage.sprite = View.LanguageFlags[_pendingLanguageIndex];

            View.LanguageLabel.text = _localizationService.GetValue(_languageConversionService.GetLocaleLanguageCodeByIndex(_pendingLanguageIndex));
        }

        private void TrySaveLanguage()
        {
            if (_pendingLanguageIndex == _currentLanguageIndex) 
                return;

            var spriteAsset = View.LanguageFlagsAssets[_pendingLanguageIndex];
        
            var context = new UniversalPopupContext(
                _localizationService.GetValue(LocalizationExtensions.ChangeLanguageNotifyKey),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetValue(LocalizationExtensions.CancelKey), null, UniversalPopupButtonStyle.Red),
                    new UniversalPopupButtonAction(_localizationService.GetValue(LocalizationExtensions.ChangeKey), ApplyLanguageChanges)
                }, _localizationService.GetValue(LocalizationExtensions.ChangeLanguageTitle), spriteAsset: spriteAsset);

            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        
            void ApplyLanguageChanges()
            {
                _currentLanguageIndex = _pendingLanguageIndex;
                //TODO SAVE PROFILE AFTER UPDATING LANGUAGE
                _userSettingsService.UpdateLanguage(_languageConversionService.GetLocaleLanguageCodeByIndex(_currentLanguageIndex));
                _reloadService.SoftRestart();
            }
        }
    }
}