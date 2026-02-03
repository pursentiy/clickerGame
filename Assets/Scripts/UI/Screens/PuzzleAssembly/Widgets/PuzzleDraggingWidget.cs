using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data.Info;
using Common.Handlers.Draggable;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using Level.Hud;
using Plugins.FSignal;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.ScreenBlocker;
using Storage.Snapshots.LevelParams;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class PuzzleDraggingWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;
        [Inject] private readonly ClickHandlerService _clickHandlerService;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly SoundHandler _soundHandler;
        
        [SerializeField] private RectTransform _draggingTransform;
        [SerializeField] private GraphicRaycaster _figuresAssemblyCanvasRaycaster;

        public FSignal CheckLevelCompletionSignal { get; } = new FSignal();
        
        private bool _isDraggable;
        private Action<bool> _lockScrollAction;
        private FigureMenuWidget _draggingMenuScrollEmptyContainer;
        private GameObject _draggingFigure;
        private FigureMenuWidget _menuWidgetFigure;
        private LevelParamsSnapshot _currentLevelParams;

        private Sequence _resetDraggingAnimationSequence;
        private Sequence _completeDraggingAnimationSequence;
        private Coroutine _finishCoroutine;
        private PackInfo _packInfo;

        public void Initialize(IReadOnlyList<FSignal<IDraggable, PointerEventData>> onBeginDragFiguresSignals,
            IReadOnlyList<FSignal<IDraggable, PointerEventData>> onDragEndFiguresSignals)
        {
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

            SubscribeToMenuFiguresSignals(onBeginDragFiguresSignals, onDragEndFiguresSignals);
        }
        
        private void SubscribeToMenuFiguresSignals(IReadOnlyList<FSignal<IDraggable, PointerEventData>> onBeginDragFiguresSignals,
            IReadOnlyList<FSignal<IDraggable, PointerEventData>> onDragEndFiguresSignals)
        {
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
            _draggingMenuScrollEmptyContainer.InitialPosition = _draggingMenuScrollEmptyContainer.transform.position;
            _draggingFigure.GetRectTransform().SetParent(_draggingTransform);
            
            _levelHudHandler.ShiftAllElements(false, figure.Id, new Promise());
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
                    var shiftingAnimationPromise = new Promise();
                    _levelHudHandler.TryShiftAllElementsAfterRemoving(_draggingMenuScrollEmptyContainer.Id, shiftingAnimationPromise);
                
                    TrySetFigureInserted(figure.Id);
                
                    _completeDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigure.transform.DOScale(0, 0.3f))
                        .KillWith(this);

                    Promise.All(shiftingAnimationPromise, _completeDraggingAnimationSequence.AsPromise(), figure.SetConnected())
                        .Then(SetMenuFigureConnected)
                        .Then(() => _coroutineService.WaitFor(0.05f))
                        .Then(() =>
                        {
                            SetDraggingFinished();
                            blockRef?.Dispose();
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
        
        private void ClearDraggingFigure()
        {
            //TODO REFACTORING MAYBE LOCK?
            //_levelHudHandler.LockScroll(false);
            _draggingMenuScrollEmptyContainer = null;
            _draggingFigure = null;
        }

        private void SetDraggingFinished()
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
            var shiftingAnimationPromise = new Promise();
            //TODO REFACTORING SHIFT
           // _levelHudHandler.ShiftAllElements(true, _draggingMenuScrollEmptyContainer.Id, shiftingAnimationPromise);

            _resetDraggingAnimationSequence = DOTween.Sequence()
                .Append(_draggingFigure.transform.DOMove(_draggingMenuScrollEmptyContainer.InitialPosition, 0.4f))
                .Join(_draggingMenuScrollEmptyContainer.transform.DOScale(1, 0.3f))
                .Join(_draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta
                    (new Vector2(_draggingMenuScrollEmptyContainer.InitialWidth, _draggingMenuScrollEmptyContainer.InitialHeight), 0.3f))
                .KillWith(this);

            Promise.All(shiftingAnimationPromise, _resetDraggingAnimationSequence.AsPromise())
                .Then(() => _levelHudHandler.ReturnFigureBackToScroll(_draggingMenuScrollEmptyContainer.Id))
                .Then(() =>
                {
                    _draggingMenuScrollEmptyContainer.FigureTransform.transform.localPosition = Vector3.zero;
                    ClearDraggingFigure();
                    return Promise.Resolved();
                })
                .Then(() => _coroutineService.WaitFor(0.15f))
                .Then(() =>
                {
                    SetDraggingFinished();
                    blockRef?.Dispose();
                })
                .Catch(e =>
                {
                    LoggerService.LogWarning(this, e.Message);
                    blockRef?.Dispose();
                })
                .CancelWith(this);
        }
    }
}