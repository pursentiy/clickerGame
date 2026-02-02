using DG.Tweening;
using Plugins.FSignal;
using RSG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Screens.PuzzleAssemblyScreen.Figures
{
    public class FigureMenuWidget : FigureBase, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private const float YDeltaDispersion = 2f;
        private const float InitialWidthParam = 250f;
        private const float InitialHeightParam = 250f;
        
        [SerializeField] protected Image _image;
        [SerializeField] protected RectTransform _transformFigure;
        [SerializeField] protected RectTransform _transformContainer;
        [SerializeField] private ParticleSystem _particleSystem;
        
        private Sequence _fadeAnimationSequence;
        private bool _isScrolling;

        public RectTransform FigureTransform => _transformFigure;
        public float InitialWidth => InitialWidthParam;
        public float InitialHeight => InitialHeightParam;
        public RectTransform ContainerTransform => _transformContainer;
        public FSignal<FigureMenuWidget> OnBeginDragFigureSignal { get; } = new FSignal<FigureMenuWidget>();
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
            
            _fadeAnimationSequence = DOTween.Sequence()
                .Append(_image.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.2f))
                .OnComplete(fadeFigurePromise.Resolve)
                .KillWith(this);
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

        public void OnDrag(PointerEventData eventData)
        {
            if (_isScrolling)
            {
                OnDraggingSignal.Dispatch(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
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
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            _particleSystem.Stop();
            
            OnEndDragSignal.Dispatch(eventData);
            
            _isScrolling = false;
        }
    }
}