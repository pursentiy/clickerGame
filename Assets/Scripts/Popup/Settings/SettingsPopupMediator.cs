using System.Collections;
using Attributes;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Popup.Settings;
using Popup.Universal;
using Services;
using Services.Player;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Utilities.Disposable;
using Zenject;

[AssetKey("UI Popups/SettingsPopupMediator")]
public class SettingsPopupMediator : UIPopupBase<SettingsPopupView>
{
    private const float SaveTogglesDelay = 1.0f;
    
    [Inject] private readonly SoundHandler _soundHandler;
    [Inject] private readonly GlobalSettingsService _globalSettingsService;
    [Inject] private readonly UIManager _uiManager;
    [Inject] private readonly ReloadService _reloadService;
    [Inject] private readonly LocalizationService _localizationService;
    [Inject] private readonly PlayerProfileManager _playerProfileManager;
    [Inject] private readonly LanguageConversionService _languageConversionService;
    [Inject] private readonly GameManager _gameManager;

    private int _currentLanguageIndex;
    private int _pendingLanguageIndex;
    private int _localesCount;
    private Coroutine _saveTogglesRoutine;
    
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
        View.MusicToggle.SetIsOnWithoutNotify(_gameManager.IsMusicOn);
        View.SoundToggle.SetIsOnWithoutNotify(_gameManager.IsSoundOn);

        View.MusicToggle.onValueChanged.MapListenerWithSound(OnMusicToggled).DisposeWith(this);
        View.SoundToggle.onValueChanged.MapListenerWithSound(OnSoundToggled).DisposeWith(this);
    }

    private void OnSoundToggled(bool isOn)
    {
        _gameManager.SetSoundAvailable(isOn);
        _soundHandler.SetSoundVolume(isOn);
        
        RestartSaveTimer();
    }
    
    private void OnMusicToggled(bool isOn)
    {
        _gameManager.SetMusicAvailable(isOn);
        _soundHandler.SetMusicVolume(isOn);
        
        RestartSaveTimer();
    }
    
    private void RestartSaveTimer()
    {
        if (_saveTogglesRoutine != null)
            StopCoroutine(_saveTogglesRoutine);

        _saveTogglesRoutine = StartCoroutine(SaveProfileWithDelayRoutine());
    }

    private IEnumerator SaveProfileWithDelayRoutine()
    {
        yield return new WaitForSeconds(SaveTogglesDelay);
        
        _playerProfileManager.SaveProfile();
        _saveTogglesRoutine = null;
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

        View.LanguageLabel.text = _localizationService.GetCommonValue(_languageConversionService.GetLocaleLanguageCodeByIndex(_pendingLanguageIndex));
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
            _currentLanguageIndex = _pendingLanguageIndex;
            //TODO SAVE PROFILE AFTER UPDATING LANGUAGE
            _gameManager.UpdateLanguage(_languageConversionService.GetLocaleLanguageCodeByIndex(_currentLanguageIndex));
            _reloadService.SoftRestart();
        }
    }

    private void OnDestroy()
    {
        if (_saveTogglesRoutine != null)
            StopCoroutine(_saveTogglesRoutine);
    }
}