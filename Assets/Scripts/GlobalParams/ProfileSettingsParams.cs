namespace GlobalParams
{
    public class ProfileSettingsParams
    {
        public bool IsMusicOn { get; set; }
        public bool IsSoundOn { get; set; }

        public ProfileSettingsParams(bool isMusicOn, bool isSoundOn)
        {
            IsMusicOn = isMusicOn;
            IsSoundOn = isSoundOn;
        }
    }
}