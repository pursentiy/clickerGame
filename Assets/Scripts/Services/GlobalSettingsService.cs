using GlobalParams;

namespace Services
{
    public class GlobalSettingsService
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
    }
}