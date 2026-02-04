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
            
            if (onBeginDragFiguresSignals.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"{nameof(onBeginDragFiguresSignals)} is null or empty at {nameof(Initialize)}");
                return;
            }
            
            if (onDragEndFiguresSignals.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"{nameof(onDragEndFiguresSignals)} is null or empty at {nameof(Initialize)}");
                return;
            }
            
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
            
            _puzzlesListWidget.TryShiftAllElements(figure.Id,false);
            _draggingMenuScrollEmptyContainer.transform.DOScale(0, 0.3f).KillWith(this);
            _puzzlesListWidget.FadeDraggingContainerOverlay(true);
            _draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta(new Vector2(0, 0), 0.3f).KillWith(this);
        }
        
        private void OnDragEndFiguresSignal(IDraggable draggable, PointerEventData eventData)
        {
            var draggingFigure = draggable?.GetAs<FigureMenuWidget>();
            if (draggingFigure == null || _draggingFigure == null) 
                return;
            if (draggingFigure.IsCompleted || draggingFigure.Id != _draggingMenuScrollEmptyContainer.Id) 
                return;

            try
            {
                var targets = _clickHandlerService.DetectFigureTarget(eventData, _figuresAssemblyCanvasRaycaster);
                var targetFigure = targets?.FirstOrDefault(f => f != null && f.Id == _draggingMenuScrollEmptyContainer.Id);

                if (targetFigure != null)
                {
                    TryInsertMenuFigure(targetFigure);
                }
                else
                {
                    ResetDraggingFigure();
                }
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(this, $"Failed to process drag end: {e.Message}");
                ResetDraggingFigure();
            }
        }

        private void TryInsertMenuFigure(FigureTargetWidget target)
        {
            var blockRef = _uiScreenBlocker.Block(15);
            _soundHandler.PlaySound("success");

            var animations = Promise.All(
                _draggingMenuScrollEmptyContainer.AnimateFigureConnection(),
                target.SetConnected(),
                _puzzlesListWidget.TryShiftAllElements(target.Id, false, true)
            );
            
            TrySetFigureConnectedSignal.Dispatch(target.Id);
            CheckLevelCompletionSignal.Dispatch();

            animations
                .Then(FinalizeInsertion)
                .Then(CleanUpAndNotify)
                .Catch(e => HandleError(e, blockRef))
                .CancelWith(this);
            
            IPromise FinalizeInsertion()
            {
                _draggingMenuScrollEmptyContainer.SetFigureCompleted(true);
                target.SetFigureCompleted(true);
                return DestroyDraggingFigure(target.Id);
            }

            void CleanUpAndNotify()
            {
                SetDraggingEnabled(false);
                _puzzlesListWidget.FadeDraggingContainerOverlay(false);
                _puzzlesListWidget.BumpDraggingContainerHolder().CancelWith(this);
                blockRef?.Dispose();
            }
        }
        
        private void ResetDraggingFigure()
        {
            var blockRef = _uiScreenBlocker.Block(15);
            _soundHandler.PlaySound("fail");
            SetDraggingEnabled(false);
            
            var animations = Promise.All(
                _puzzlesListWidget.TryShiftAllElements(_draggingMenuScrollEmptyContainer.Id, isInserting: true)
            );

            _puzzlesListWidget.AnimateMenuFigureFlightToPosition(_draggingMenuScrollEmptyContainer, _draggingFigure)
                .CancelWith(this);

           _puzzlesListWidget.FadeDraggingContainerOverlay(false, 0.55f).CancelWith(this);
            animations
                .Then(FinalizeFigureReturn)
                .Then(() =>
                {
                    _puzzlesListWidget.BumpDraggingContainerHolder().CancelWith(this);
                    blockRef?.Dispose();
                })
                .Catch(HandleResetError)
                .CancelWith(this);
            
            IPromise FinalizeFigureReturn()
            {
                _puzzlesListWidget.ReturnFigureBackToScroll(_draggingMenuScrollEmptyContainer.Id);
                _draggingMenuScrollEmptyContainer.SetFigureTransformPosition(Vector3.zero);
                ClearDraggingFigureElements();
                return Promise.Resolved();
            }

            void HandleResetError(Exception e)
            {
                LoggerService.LogWarning(this, e.Message);
                blockRef?.Dispose();
            }
        }

        private void HandleError(Exception e, IDisposable blockRef)
        {
            LoggerService.LogWarning(this, $"Insert error: {e.Message}");
            blockRef?.Dispose();
        }
        
        private void ClearDraggingFigureElements()
        {
            _draggingMenuScrollEmptyContainer = null;
            _draggingFigure = null;
        }
        
        private IPromise DestroyDraggingFigure(int figureId)
         {
             _puzzlesListWidget.DestroyFigure(figureId);
             ClearDraggingFigureElements();
             
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