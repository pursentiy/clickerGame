using Handlers.UISystem.Screens;
using Storage.Snapshots.LevelParams;

namespace UI.Screens.PuzzleAssembly
{
    public class PuzzleAssemblyScreenContext : IScreenContext
    {
        public PuzzleAssemblyScreenContext(LevelParamsSnapshot levelParamsSnapshot, int packId)
        {
            LevelParamsSnapshot = levelParamsSnapshot;
            PackId = packId;
        }

        public LevelParamsSnapshot LevelParamsSnapshot { get; }
        public int PackId { get; }
    }
}