using UI.Screens.PuzzleAssembly.Figures;

namespace Common.Data.Info
{
    public class FigureInfo
    {
        public int FigureId;
        public FigureTargetWidget FigureTargetWidget;
        public FigureMenuWidget FigureMenuWidget;

        public FigureInfo(int figureId, FigureTargetWidget _figureTargetWidget, FigureMenuWidget figureMenuWidget)
        {
            FigureId = figureId;
            FigureTargetWidget = _figureTargetWidget;
            FigureMenuWidget = figureMenuWidget;
        }
    }
}