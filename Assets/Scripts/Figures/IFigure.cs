using UnityEngine;

namespace Figures
{
    public interface IFigure
    {
        void SetUpDefaultParamsFigure(Color figureColor, FigureType type);
        void GetPoolObjectComponent();
    }
}