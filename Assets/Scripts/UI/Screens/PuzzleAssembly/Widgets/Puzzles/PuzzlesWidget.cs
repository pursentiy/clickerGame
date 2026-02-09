using System;
using System.Collections.Generic;
using System.Linq;
using Common.Handlers.Draggable;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using Plugins.FSignal;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.ScreenBlocker;
using Storage;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;
using AudioExtensions = Extensions.AudioExtensions;

namespace UI.Screens.PuzzleAssembly.Widgets.Puzzles
{
    public class PuzzlesWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker; 
        [Inject] private readonly ClickHandlerService _clickHandlerService;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly SoundHandler _soundHandler;
        
        [SerializeField] private PuzzlesListWidget _puzzlesListWidget;
        [SerializeField] private RectTransform _draggingTransform;
        [SerializeField] private RectTransform _figuresAssemblyContainer;
        [SerializeField] private GraphicRaycaster _figuresAssemblyCanvasRaycaster;

        public FSignal CheckLevelCompletionSignal { get; } = new();
        public FSignal<int> TrySetFigureConnectedSignal { get; } = new();
        
        private bool _isDraggable;
        private Action<bool> _lockScrollAction;

        private FigureMenuWidget _draggingMenuScrollEmptyContainer;
        private GameObject _draggingFigure;
        
        public void Initialize(List<TargetFigureInfo> figuresTargetList, 
            List<MenuFigureInfo> figuresMenuList, float assemblyContainerScale)
        {
            SetAssemblyContainerScale(assemblyContainerScale);
            InitializePuzzles(figuresTargetList, figuresMenuList);
            SubscribeToMenuFiguresSignals();
        }

        private void InitializePuzzles(List<TargetFigureInfo> figuresTargetList, List<MenuFigureInfo> figuresMenuList)
        {
            _puzzlesListWidget.Initialize(figuresTargetList, figuresMenuList);
        }
        
        private void SetAssemblyContainerScale(float scale)
        {
            if (scale <= 0)
            {
                LoggerService.LogWarning(this, $"Scale must be greater than zero at {nameof(SetAssemblyContainerScale)}");
                return;
            }
            
            _figuresAssemblyContainer.localScale = new Vector3(scale, scale, scale);
        }
        
        private void SubscribeToMenuFiguresSignals()
        {
            var onBeginDragFiguresSignals = _puzzlesListWidget.OnBeginDragFiguresSignals;
            var onDragEndFiguresSignals = _puzzlesListWidget.OnDragEndFiguresSignals;
            
            if (onBeginDragFiguresSignals.IsCollectionNullOrEmpty()) return;
            if (onDragEndFiguresSignals.IsCollectionNullOrEmpty()) return;
            
            foreach (var beginSignal in onBeginDragFiguresSignals)
            {
                beginSignal.MapListener(OnBeginDragFiguresSignal).DisposeWith(this);
            }
            
            foreach (var endSignal in onDragEndFiguresSignals)
            {
                endSignal.MapListener(OnDragEndFiguresSignal).DisposeWith(this);
            }
        }
        
        private void OnBeginDragFiguresSignal(IDraggable draggable, PointerEventData eventData)
        {
            if (_isDraggable || draggable == null)
            {
                return;
            }
            
            var figure = draggable.GetAs<FigureMenuWidget>();
            if (figure == null || figure.IsCompleted)
                return;

            SetDraggingEnabled(true);
            _lockScrollAction?.SafeInvoke(true);
            
            _draggingMenuScrollEmptyContainer = figure;
            _draggingFigure = _draggingMenuScrollEmptyContainer.FigureTransform.gameObject;
            _draggingMenuScrollEmptyContainer.SetInitialPosition(_draggingMenuScrollEmptyContainer.transform.position);
            _draggingFigure.GetRectTransform().SetParent(_draggingTransform);
            
            _puzzlesListWidget.ReflowLayout(hiddenFigureId: figure.Id);
            
            _draggingMenuScrollEmptyContainer.transform.DOScale(0, 0.3f).KillWith(this);
            _puzzlesListWidget.FadeDraggingContainerOverlay(true);
            _draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta(new Vector2(0, 0), 0.3f).KillWith(this);
        }
        
        private void OnDragEndFiguresSignal(IDraggable draggable, PointerEventData eventData)
        {
            var draggingFigureWidget = draggable?.GetAs<FigureMenuWidget>();
            
            if (draggingFigureWidget == null || _draggingFigure == null) 
                return;
            if (draggingFigureWidget.IsCompleted || draggingFigureWidget.Id != _draggingMenuScrollEmptyContainer.Id) 
                return;
            
            var figureToAnimate = _draggingMenuScrollEmptyContainer;
            var visualObjectToAnimate = _draggingFigure;

            ClearDraggingFigureElements();
            SetDraggingEnabled(false);
            
            _lockScrollAction?.SafeInvoke(false);

            try
            {
                var targets = _clickHandlerService.DetectFigureTarget(eventData, _figuresAssemblyCanvasRaycaster);
                var targetFigure = targets?.FirstOrDefault(f => f != null && f.Id == figureToAnimate.Id);

                if (targetFigure != null)
                {
                    TryInsertMenuFigure(figureToAnimate, visualObjectToAnimate, targetFigure);
                }
                else
                {
                    ResetDraggingFigure(figureToAnimate, visualObjectToAnimate);
                }
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(this, $"Failed to process drag end: {e.Message}");
                ResetDraggingFigure(figureToAnimate, visualObjectToAnimate);
            }
        }

        private void TryInsertMenuFigure(FigureMenuWidget figureWidget, GameObject visualObj, FigureTargetWidget target)
        {
            _soundHandler.PlaySound(AudioExtensions.SuccessPuzzleInsertionKey);

            var animations = Promise.All(
                figureWidget.AnimateFigureConnection(),
                target.SetConnected(),
                _puzzlesListWidget.ReflowLayout(target.Id)
            );
            
            TrySetFigureConnectedSignal.Dispatch(target.Id);
            CheckLevelCompletionSignal.Dispatch();

            if (!_isDraggable)
            {
                 _puzzlesListWidget.FadeDraggingContainerOverlay(false);
            }
            
            animations
                .Then(FinalizeInsertion)
                .Catch(HandleError)
                .CancelWith(this);
            
            IPromise FinalizeInsertion()
            {
                figureWidget.SetFigureCompleted(true);
                target.SetFigureCompleted(true);
                return DestroyDraggingFigure(figureWidget, visualObj);
            }
        }
        
        private void ResetDraggingFigure(FigureMenuWidget figureWidget, GameObject visualObj)
        {
            
            _soundHandler.PlaySound(AudioExtensions.FailPuzzleInsertionKey);
            
            if (!_isDraggable)
            {
                _puzzlesListWidget.FadeDraggingContainerOverlay(false, 0.55f).CancelWith(this);
            }
            
            var animations = Promise.All(
                _puzzlesListWidget.ReflowLayout(hiddenFigureId: null),
                _puzzlesListWidget.AnimateMenuFigureFlightToPosition(figureWidget, visualObj)
            );

            animations
                .Then(FinalizeFigureReturn)
                .Catch(HandleResetError)
                .CancelWith(this);
            
            IPromise FinalizeFigureReturn()
            {
                _puzzlesListWidget.ReturnFigureBackToScroll(figureWidget.Id);
                figureWidget.SetFigureTransformPosition(Vector3.zero);
                
                return Promise.Resolved();
            }

            void HandleResetError(Exception e)
            {
                LoggerService.LogWarning(this, e.Message);
            }
        }

        private void HandleError(Exception e)
        {
            LoggerService.LogWarning(this, $"Insert error: {e.Message}");
        }
        
        private void ClearDraggingFigureElements()
        {
            _draggingMenuScrollEmptyContainer = null;
            _draggingFigure = null;
        }
        
        private IPromise DestroyDraggingFigure(FigureMenuWidget figureWidget, GameObject visualObj)
        {
            var idToRemove = figureWidget.Id;

            if (_puzzlesListWidget.DestroyFigure(idToRemove))
            {
                _puzzlesListWidget.ReflowLayout(null);
            }
     
            if(visualObj != null) Destroy(visualObj);
            if(figureWidget != null) figureWidget.DestroyWidget();
     
            return _coroutineService.WaitFrame();
        }

        private void SetDraggingEnabled(bool enable)
        {
            _isDraggable = enable;
        }

        private void Update()
        {
            TryUpdateDraggingFigurePosition();
        }

        private void TryUpdateDraggingFigurePosition()
        {
            if (_draggingMenuScrollEmptyContainer == null || _draggingFigure == null || !_isDraggable)
            {
                return;
            }

            _draggingFigure.transform.position = Input.mousePosition;
        }
    }
}