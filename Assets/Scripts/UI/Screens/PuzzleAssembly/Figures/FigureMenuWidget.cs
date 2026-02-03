using Common.Handlers.Draggable;
using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Screens.PuzzleAssembly.Figures
{
    public class FigureMenuWidget : DraggableItemHandler, IFigureBase
    {
        private const float YDeltaDispersion = 2f;
        private const float InitialWidthParam = 250f;
        private const float InitialHeightParam = 250f;
        
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _transformFigure;
        [SerializeField] private RectTransform _transformContainer;
        [SerializeField] private ParticleSystem _particleSystem;
        
        private Sequence _fadeAnimationSequence;
        private bool _isScrolling;
        
        public bool IsCompleted { get; private set; }
        public RectTransform FigureTransform => _transformFigure;
        public float InitialWidth => InitialWidthParam;
        public float InitialHeight => InitialHeightParam;
        public RectTransform ContainerTransform => _transformContainer;
        public void SetFigureCompleted(bool value)
        {
            IsCompleted = value;
        }
        
        public Vector3 InitialPosition { get; set; }

        //TODO REFACTORING DO NEED THIS?
        public void SetScale(float scale)
        {
            //_transformFigure.localScale = new Vector3(scale, scale, 0);
        }

        public IPromise SetConnected()
        {
            SetFigureCompleted(true);
            return FadeFigure();
        }
        
        //TODO REFACTORING DO NEED THIS?
        private void Start()
        {
            //ContainerTransform.sizeDelta = new Vector2(InitialWidthParam, InitialHeightParam);
        }

        public void DestroyWidget()
        {
            if (_transformFigure != null)
                Destroy(_transformFigure.gameObject);
            
            Destroy(gameObject);
        }

        protected override void OnBeginDragInternally(PointerEventData eventData)
        {
            base.OnBeginDragInternally(eventData);
            
            if (_particleSystem != null && eventData.delta.y >= YDeltaDispersion)
            {
                _particleSystem.Simulate(0);
                _particleSystem.Play();
            }
        }

        protected override void OnEndDragInternally(PointerEventData eventData)
        {
            base.OnEndDragInternally(eventData);
            
            _particleSystem.Stop();
        }
        
        private IPromise FadeFigure()
        {
            var color = _image.color;

            _fadeAnimationSequence?.Kill(true);
            _fadeAnimationSequence = DOTween.Sequence()
                .Append(_image.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.2f))
                .KillWith(this);

            return _fadeAnimationSequence.AsPromise();
        }
    }
}