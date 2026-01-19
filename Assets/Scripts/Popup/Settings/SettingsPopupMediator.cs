using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Popup.Settings;
using Popup.Universal;
using Services;
using Services.Player;
using Utilities.Disposable;
using Zenject;

[AssetKey("UI Popups/SettingsPopupMediator")]
public class SettingsPopupMediator : UIPopupBase<SettingsPopupView>
{
    [Inject] private readonly SoundHandler _soundHandler;
    [Inject] private readonly GameSettingsManager _gameSettingsManager;
    [Inject] private readonly UIManager _uiManager;
    [Inject] private readonly ReloadService _reloadService;
    [Inject] private readonly LocalizationService _localizationService;
    [Inject] private readonly PlayerProfileManager _playerProfileManager;
    [Inject] private readonly LanguageConversionService _languageConversionService;
    [Inject] private readonly GameParamsManager _gameParamsManager;

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
        
        View.LeftLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(-1)).DisposeWith(this);
        View.RightLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(1)).DisposeWith(this);
        View.SaveLanguageButton.onClick.MapListenerWithSound(TrySaveLanguage).DisposeWith(this);
        View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
        View.BackgroundButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
    }

    private void SetupToggles()
    {
        View.MusicToggle.SetIsOnWithoutNotify(_gameParamsManager.IsMusicOn);
        View.SoundToggle.SetIsOnWithoutNotify(_gameParamsManager.IsSoundOn);

        View.MusicToggle.onValueChanged.MapListenerWithSound(OnMusicToggled).DisposeWith(this);
        View.SoundToggle.onValueChanged.MapListenerWithSound(OnSoundToggled).DisposeWith(this);
    }

    private void OnSoundToggled(bool isOn)
    {
        _gameParamsManager.SetSoundAvailable(isOn);
        _soundHandler.SetSoundVolume(isOn);
    }
    
    private void OnMusicToggled(bool isOn)
    {
        _gameParamsManager.SetMusicAvailable(isOn);
        _soundHandler.SetMusicVolume(isOn);
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
            _gameParamsManager.UpdateLanguage(_languageConversionService.GetLocaleLanguageCodeByIndex(_currentLanguageIndex));
            _reloadService.SoftRestart();
        }
    }
}