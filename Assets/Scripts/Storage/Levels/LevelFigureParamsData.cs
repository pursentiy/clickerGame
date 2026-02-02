using System;
using UI.Screens.PuzzleAssemblyScreen.Figures;

namespace Storage.Levels
{
    [Serializable]
    public class LevelFigureParamsData
    {
        public int FigureId;
        public FigureTargetWidget _figureTargetWidget;
        public FigureMenuWidget figureMenuWidget;
    }
}