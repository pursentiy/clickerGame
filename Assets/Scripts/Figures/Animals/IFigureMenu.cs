using Plugins.FSignal;
using RSG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Figures.Animals
{
    public interface IFigureMenu
    {
        int SiblingPosition { get; set;}
        Vector3 InitialPosition { get; set; }
        void SetScale(float scale);
        FSignal<PointerEventData> OnBeginDragSignal { get; }
        FSignal<PointerEventData> OnEndDragSignal { get; }
        FSignal<PointerEventData> OnDraggingSignal { get; }
        FSignal<FigureMenu> OnBeginDragFigureSignal { get; }
        RectTransform FigureTransform { get; }
        float InitialWidth { get; }
        float InitialHeight { get; }
        RectTransform ContainerTransform { get; }
        void SetConnected(Promise fadeFigurePromise);
        void Destroy();
    }
}