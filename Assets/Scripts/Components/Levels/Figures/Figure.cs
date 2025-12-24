using UnityEngine;

namespace Components.Levels.Figures
{
    public abstract class Figure : MonoBehaviour
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
