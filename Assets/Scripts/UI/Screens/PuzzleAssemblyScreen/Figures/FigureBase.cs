using UnityEngine;

namespace UI.Screens.PuzzleAssemblyScreen.Figures
{
    public abstract class FigureBase : MonoBehaviour
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
