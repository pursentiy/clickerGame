using System;
using DG.Tweening;
using Plugins.FSignal;
using RSG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Figures.Animals
{
    public class FigureMenu : Figure, IFigureMenu, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] protected Image _image;
        [SerializeField] protected RectTransform _transformFigure;
        [SerializeField] protected RectTransform _transformContainer;
        [SerializeField] private ParticleSystem _particleSystem;
        
        private const float YDeltaDispersion = 2f;
        public const float InitialWidthParam = 250f;
        public const float InitialHeightParam = 250f;
        
        private Sequence _fadeAnimationSequence;
        private bool _isScrolling;

        public RectTransform FigureTransform => _transformFigure;
        public float InitialWidth => InitialWidthParam;
        public float InitialHeight => InitialHeightParam;
        public RectTransform ContainerTransform => _transformContainer;
        public FSignal<FigureMenu> OnBeginDragFigureSignal { get; } = new FSignal<FigureMenu>();
        public FSignal<PointerEventData> OnBeginDragSignal { get; } = new FSignal<PointerEventData>();
        public FSignal<PointerEventData> OnDraggingSignal { get; } = new FSignal<PointerEventData>();
        public FSignal<PointerEventData> OnEndDragSignal { get; } = new FSignal<PointerEventData>();

        public int SiblingPosition { get; set; }
        public Vector3 InitialPosition { get; set; }

        private void Start()
        {
            ContainerTransform.sizeDelta = new Vector2(InitialWidthParam, InitialHeightParam);
        }

        public void SetScale(float scale)
        {
            _transformFigure.localScale = new Vector3(scale, scale, 0);
        }

        private void FadeFigure(Promise fadeFigurePromise)
        {
            var color = _image.color;
            _fadeAnimationSequence = DOTween.Sequence().Append(_image.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.2f)).OnComplete(
                fadeFigurePromise.Resolve);
        }

        public void SetConnected(Promise fadeFigurePromise)
        {
            SetFigureCompleted(true);
            FadeFigure(fadeFigurePromise);
            
        }

        public void Destroy()
        {
            Destroy(gameObject);
            
            if(_transformFigure != null)
                Destroy(_transformFigure.gameObject);
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
                _particleSystem.Simulate(0);
                _particleSystem.Play();
                
                OnBeginDragFigureSignal.Dispatch(this);
                return;
            }

            OnBeginDragSignal.Dispatch(eventData);
            _isScrolling = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _particleSystem.Stop();
            
            OnEndDragSignal.Dispatch(eventData);
            
            _isScrolling = false;
        }
    }
}