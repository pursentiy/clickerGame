using Components.Levels.Figures;
using UnityEngine.EventSystems;

namespace Level.Click
{
    public interface IClickHandler
    {
        FigureTarget TryGetFigureAnimalTargetOnDragEnd(PointerEventData eventData);
    }
}