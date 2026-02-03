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

            _isDraggable = true;
            _lockScrollAction?.SafeInvoke(true);
            
            _draggingMenuScrollEmptyContainer = figure;
            _draggingFigure = _draggingMenuScrollEmptyContainer.FigureTransform.gameObject;
            _draggingMenuScrollEmptyContainer.SetInitialPosition(_draggingMenuScrollEmptyContainer.transform.position);
            _draggingFigure.GetRectTransform().SetParent(_draggingTransform);
            
            _puzzlesListWidget.TryShiftAllElements(figure.Id,false);
            _draggingMenuScrollEmptyContainer.transform.DOScale(0, 0.3f).KillWith(this);
            _draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta(new Vector2(0, 0), 0.3f).KillWith(this);
        }
        
        private void OnDragEndFiguresSignal(IDraggable draggable, PointerEventData eventData)
        {
            if (_draggingFigure == null)
                return;
            
            var draggingFigure = draggable.GetAs<FigureMenuWidget>();
            if (draggingFigure == null || draggingFigure.IsCompleted)
                return;

            if (draggingFigure.Id != _draggingMenuScrollEmptyContainer.Id)
                return;

            try
            {
                var maybeTargetFigures = _clickHandlerService.DetectFigureTarget(eventData, _figuresAssemblyCanvasRaycaster);
                if (maybeTargetFigures.IsCollectionNullOrEmpty())
                {
                    ResetDraggingFigure();
                    return;
                }
            
                var figure = maybeTargetFigures.FirstOrDefault(i => i != null && i.Id == _draggingMenuScrollEmptyContainer.Id);
                if (figure == null)
                {
                    ResetDraggingFigure();
                    return;
                }
                
                if (_draggingMenuScrollEmptyContainer.Id == figure.Id)
                {
                    var blockRef = _uiScreenBlocker.Block(15);
                    _soundHandler.PlaySound("success");
                    _puzzlesListWidget.FinishAllShiftingAnimations();
                    
                    var figureScaleTweener = _draggingFigure.transform.DOScale(0, 0.3f).KillWith(this);
                    var fadePromise = _draggingMenuScrollEmptyContainer.FadeFigure();
                    
                    Promise.All(fadePromise, figureScaleTweener.AsPromise(), figure.SetConnected())
                        .Then(() =>
                        {
                            _draggingMenuScrollEmptyContainer.SetFigureCompleted(true);
                            figure.SetFigureCompleted(true);
                            return Promise.Resolved();
                        })
                        .Then(() => DestroyDraggingFigure(figure.Id))
                        .Then(() => _coroutineService.WaitFor(0.05f))
                        .Then(() =>
                        {
                            SetDraggingDisabled();
                            blockRef?.Dispose();
                            TrySetFigureConnectedSignal.Dispatch(figure.Id);
                            CheckLevelCompletionSignal.Dispatch();
                        })
                        .Catch(e =>
                        {
                            LoggerService.LogWarning(this, e.Message);
                            blockRef?.Dispose();
                        })
                        .CancelWith(this);
                }
                else
                {
                    ResetDraggingFigure();
                }
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(this, e.Message);
            }
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

        private void SetDraggingDisabled()
        {
            _isDraggable = false;
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
        
        private void ResetDraggingFigure()
        {
            var blockRef = _uiScreenBlocker.Block(15);
            _soundHandler.PlaySound("fail");
            
            SetDraggingDisabled();
            
            var shiftingAnimationPromise = _puzzlesListWidget.TryShiftAllElements(_draggingMenuScrollEmptyContainer.Id, true);
            var figureFlightPromise = _puzzlesListWidget.AnimateMenuFigureFlightToPosition(_draggingMenuScrollEmptyContainer, _draggingFigure);

            Promise.All(shiftingAnimationPromise, figureFlightPromise)
                .Then(() => _puzzlesListWidget.ReturnFigureBackToScroll(_draggingMenuScrollEmptyContainer.Id))
                .Then(() =>
                {
                    _draggingMenuScrollEmptyContainer.FigureTransform.transform.localPosition = Vector3.zero;
                    ClearDraggingFigureElements();
                    return Promise.Resolved();
                })
                .Then(() => _coroutineService.WaitFor(0.15f))
                .Then(() => blockRef?.Dispose())
                .Catch(e =>
                {
                    LoggerService.LogWarning(this, e.Message);
                    blockRef?.Dispose();
                })
                .CancelWith(this);
        }
    }
}