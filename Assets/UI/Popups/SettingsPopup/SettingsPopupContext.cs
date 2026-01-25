using Handlers.UISystem.Popups;

namespace UI.Popups.SettingsPopup
{
    public class SettingsPopupContext : IPopupContext
    {
        public SettingsPopupContext(bool allowLanguageChanging)
        {
            AllowLanguageChanging = allowLanguageChanging;
        }

        public bool AllowLanguageChanging { get; }
    }
}