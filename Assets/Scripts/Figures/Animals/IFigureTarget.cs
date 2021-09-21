using UnityEngine;

namespace Figures.Animals
{
    public interface IFigureTarget
    {
        void SetUpFigure(bool isCompleted);
        void SetConnected();
    }
}