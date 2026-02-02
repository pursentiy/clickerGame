using Handlers.UISystem.Screens;
using Storage.Snapshots.LevelParams;

namespace UI.Screens.PuzzleAssemblyScreen
{
    public class PuzzleAssemblyScreenContext : IScreenContext
    {
        public PuzzleAssemblyScreenContext(LevelParamsSnapshot levelParamsSnapshot)
        {
            LevelParamsSnapshot = levelParamsSnapshot;
        }

        public LevelParamsSnapshot  LevelParamsSnapshot { get; }
    }
}