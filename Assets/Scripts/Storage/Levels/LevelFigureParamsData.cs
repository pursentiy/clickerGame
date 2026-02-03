using System;
using UI.Screens.PuzzleAssembly.Figures;

namespace Storage.Levels
{
    [Serializable]
    public class LevelFigureParamsData
    {
        public int FigureId;
        public FigureTargetWidget FigureTarget;
        public FigureMenuWidget FigureMenu;
    }
}