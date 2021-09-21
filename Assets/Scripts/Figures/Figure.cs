using Pooling;
using UnityEngine;

namespace Figures
{
    public abstract class Figure : MonoBehaviour, IFigure
    {
        public int FigureId { get; private set; }
        public bool IsCompleted { get; private set; }

        public void SetFigureCompleted(bool value)
        {
            IsCompleted = value;
        }

        public void SetUpDefaultParamsFigure(int figureId)
        {
            FigureId = figureId;
        }
    }
}
