using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Popup.Settings;
using Popup.Universal;
using Services;
using UnityEngine.Localization.Settings;
using Utilities.Disposable;
using Zenject;

[AssetKey("UI Popups/SettingsPopupMediator")]
public class SettingsPopupMediator : UIPopupBase<SettingsPopupView>
{
    [Inject] private readonly SoundHandler _soundHandler;
    [Inject] private readonly GlobalSettingsService _globalSettingsService;
    [Inject] private readonly UIManager _uiManager;
    [Inject] private readonly ReloadService _reloadService;
    [Inject] private readonly LocalizationService _localizationService;

    private int _currentLanguageIndex;
    private int _pendingLanguageIndex;
    private int LocalesCount => LocalizationSettings.AvailableLocales.Locales.Count;
    
    public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

    public override void OnCreated()
    {
        base.OnCreated();
        
        // Получаем текущий индекс напрямую из настроек локализации
        _currentLanguageIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        if (_currentLanguageIndex < 0) _currentLanguageIndex = 0;
        
        _pendingLanguageIndex = _currentLanguageIndex;
        
        RefreshLanguageUI();
        SetupToggles();
        
        View.LeftLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(-1)).DisposeWith(this);
        View.RightLanguageButton.onClick.MapListenerWithSound(() => ChangePendingIndex(1)).DisposeWith(this);
        View.SaveLanguageButton.onClick.MapListenerWithSound(TrySaveLanguage).DisposeWith(this);
        View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
    }

    private void SetupToggles()
    {
        View.MusicToggle.SetIsOnWithoutNotify(_globalSettingsService.ProfileSettingsMusic);
        View.SoundToggle.SetIsOnWithoutNotify(_globalSettingsService.ProfileSettingsSound);

        View.MusicToggle.onValueChanged.MapListenerWithSound(isOn => {
            _globalSettingsService.ProfileSettingsMusic = isOn;
            _soundHandler.SetMusicVolume(isOn);
        }).DisposeWith(this);

        View.SoundToggle.onValueChanged.MapListenerWithSound(isOn => {
            _globalSettingsService.ProfileSettingsSound = isOn;
            _soundHandler.SetSoundVolume(isOn);
        }).DisposeWith(this);
    }

    private void ChangePendingIndex(int direction)
    {
        _pendingLanguageIndex = (_pendingLanguageIndex + direction + LocalesCount) % LocalesCount;
        RefreshLanguageUI();
    }

    private void RefreshLanguageUI()
    {
        View.SaveLanguageButton.TrySetActive(_pendingLanguageIndex != _currentLanguageIndex);
        
        if (View.LanguageFlags.Length > _pendingLanguageIndex)
            View.CountryFlagImage.sprite = View.LanguageFlags[_pendingLanguageIndex];

        if (!LocalizationSettings.AvailableLocales.Locales.IsNullOrEmpty()
            && _pendingLanguageIndex >= 0
            && _pendingLanguageIndex < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            View.LanguageLabel.text = _localizationService.GetCommonValue(LocalizationSettings.AvailableLocales.Locales[_pendingLanguageIndex].LocaleName);
        }
    }

    private void TrySaveLanguage()
    {
        if (_pendingLanguageIndex == _currentLanguageIndex) 
            return;

        var spriteAsset = View.LanguageFlagsAssets[_pendingLanguageIndex];
        
        var context = new UniversalPopupContext(
            _localizationService.GetCommonValue(LocalizationExtensions.ChangeLanguageNotifyKey),
            new[] {
                new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.CancelKey), null, UniversalPopupButtonStyle.Red),
                new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.ChangeKey), ApplyLanguageChanges)
            }, _localizationService.GetCommonValue(LocalizationExtensions.ChangeLanguageTitle), spriteAsset: spriteAsset);

        _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        
        void ApplyLanguageChanges()
        {
            _currentLanguageIndex =  _pendingLanguageIndex;
            UpdateLocalizationSettings(_currentLanguageIndex);
            _reloadService.SoftRestart();
        }
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