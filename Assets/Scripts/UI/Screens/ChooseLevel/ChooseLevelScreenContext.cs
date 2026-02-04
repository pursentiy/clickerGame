using Common.Data.Info;
using Handlers.UISystem.Screens;

namespace UI.Screens.ChooseLevel
{
    public class ChooseLevelScreenContext : IScreenContext
    {
        public ChooseLevelScreenContext(PackInfo packInfo)
        {
            PackInfo = packInfo;
        }

        public PackInfo PackInfo { get; }
    }
}