using Plugins.FSignal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Figures.Animals
{
    public interface IFigureAnimalsMenu
    {
        void SetUpFigure(Sprite sprite, Color color);
        int SiblingPosition { get; set;}
        Vector3 InitialPosition { get; set; }
        void SetScale(float scale);
        FSignal<PointerEventData> OnBeginDragSignal { get; }
        FSignal<PointerEventData> OnEndDragSignal { get; }
        FSignal<PointerEventData> OnDraggingSignal { get; }
        FSignal<FigureAnimalsMenu> OnBeginDragFigureSignal { get; }
        void SetConnected();
        void Destroy();
    }
}