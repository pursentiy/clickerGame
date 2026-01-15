using GlobalParams;
using Services.Base;
using UnityEngine;

namespace Services
{
    public class GlobalSettingsService : DisposableService
    {
        private ProfileSettingsParams _profileSettings;
        
        public bool ProfileSettingsSound
        {
            get => _profileSettings.IsSoundOn;
            set => _profileSettings.IsSoundOn = value;
        }

        public bool ProfileSettingsMusic
        {
            get => _profileSettings.IsMusicOn;
            set => _profileSettings.IsMusicOn = value;
        }
        
        public void InitializeProfileSettings()
        {
            _profileSettings = new ProfileSettingsParams(true, true);
        }

        protected override void OnInitialize()
        {
            Input.multiTouchEnabled = false;
        }

        protected override void OnDisposing()
        {
            
        }
    }
}