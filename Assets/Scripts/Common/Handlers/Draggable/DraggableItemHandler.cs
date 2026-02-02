using Plugins.FSignal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.Handlers.Draggable
{
    public class DraggableItemHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDraggable
    {
        private const PointerEventData.InputButton ForbiddenButton = PointerEventData.InputButton.Left;
        public FSignal<IDraggable, PointerEventData> OnBeginDragSignal { get; } = new ();
        public FSignal<PointerEventData> OnDragSignal { get; } = new ();
        public FSignal<PointerEventData> OnEndDragSignal { get; } = new ();
        public int Id { get; private set; }
        
        private bool _isDragging;

        public void Initialize(int id)
        {
            Id = id;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            OnDragSignal.Dispatch(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != ForbiddenButton)
                return;
            
            _isDragging = true;
            OnBeginDragSignal.Dispatch(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            OnEndDragSignal.Dispatch(eventData);
        }
    }
}