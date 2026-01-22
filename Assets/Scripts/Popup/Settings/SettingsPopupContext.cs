using Handlers.UISystem.Popups;

namespace Popup.Settings
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