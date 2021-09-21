using System;
using Figures.Animals;

namespace Storage.Levels
{
    [Serializable]
    public class LevelFigureParamsData
    {
        public int FigureId;
        public FigureTarget FigureTarget;
        public FigureMenu FigureMenu;
    }
}