using Handlers;
using UnityEngine;
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

            _closeButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _popupHandler.HideCurrentPopup();
            });
        }

        private void SetupMusicAndSoundToggles()
        {
            _musicToggle.isOn = _progressHandler.ProfileSettingsMusic;
            _soundToggle.isOn = _progressHandler.ProfileSettingsSound;
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _musicToggle.onValueChanged.RemoveAllListeners();
            _soundToggle.onValueChanged.RemoveAllListeners();
        }
    }
}