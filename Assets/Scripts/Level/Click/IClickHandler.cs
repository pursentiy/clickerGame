using Figures.Animals;
using Plugins.FSignal;
using UnityEngine.EventSystems;

namespace Level.Click
{
    public interface IClickHandler
    {
        FigureTarget TryGetFigureAnimalTargetOnDragEnd(PointerEventData eventData);
    }
}