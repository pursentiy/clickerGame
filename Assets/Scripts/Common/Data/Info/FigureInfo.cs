using Components.Levels.Figures;

namespace Common.Data.Info
{
    public class FigureInfo
    {
        public int FigureId;
        public FigureTarget FigureTarget;
        public FigureMenu FigureMenu;

        public FigureInfo(int figureId, FigureTarget figureTarget, FigureMenu figureMenu)
        {
            FigureId = figureId;
            FigureTarget = figureTarget;
            FigureMenu = figureMenu;
        }
    }
}