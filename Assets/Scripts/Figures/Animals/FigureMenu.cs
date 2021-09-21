using DG.Tweening;
using Plugins.FSignal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Figures.Animals
{
    public class FigureMenu : Figure, IFigureMenu, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] protected Image _image;
        [SerializeField] protected RectTransform _transform;
        
        private const float YDeltaDispersion = 2f;
        
        private Sequence _fadeAnimationSequence;
        private bool _isScrolling;

        public FSignal<FigureMenu> OnBeginDragFigureSignal { get; } = new FSignal<FigureMenu>();
        public FSignal<PointerEventData> OnBeginDragSignal { get; } = new FSignal<PointerEventData>();
        public FSignal<PointerEventData> OnDraggingSignal { get; } = new FSignal<PointerEventData>();
        public FSignal<PointerEventData> OnEndDragSignal { get; } = new FSignal<PointerEventData>();

        public int SiblingPosition { get; set; }
        public Vector3 InitialPosition { get; set; }

        public void SetScale(float scale)
        {
            _transform.localScale = new Vector3(scale, scale, 0);
        }

        private void FadeFigure()
        {
            var color = _image.color;
            
            _fadeAnimationSequence = DOTween.Sequence().Append(_image.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.2f));
        }

        public void SetConnected()
        {
            FadeFigure();
            SetFigureCompleted(true);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _fadeAnimationSequence?.Kill();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isScrolling)
            {
                OnDraggingSignal.Dispatch(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData.delta.y < YDeltaDispersion) )
            {
                OnBeginDragFigureSignal.Dispatch(this);
                return;
            }
            
            OnBeginDragSignal.Dispatch(eventData);
            _isScrolling = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragSignal.Dispatch(eventData);
            
            _isScrolling = false;
        }
    }
}