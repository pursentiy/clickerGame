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
        public FSignal<IDraggable, PointerEventData> OnEndDragSignal { get; } = new ();
        public int Id { get; private set; }
        
        private bool _isDragging;

        public virtual void Initialize(int id)
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
            OnBeginDragInternally(eventData);
            OnBeginDragSignal.Dispatch(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            
            OnEndDragInternally(eventData);
            OnEndDragSignal.Dispatch(this, eventData);
        }

        protected virtual void OnEndDragInternally(PointerEventData eventData)
        {
            
        }
        
        protected virtual void OnBeginDragInternally(PointerEventData eventData)
        {
            
        }
    }
}