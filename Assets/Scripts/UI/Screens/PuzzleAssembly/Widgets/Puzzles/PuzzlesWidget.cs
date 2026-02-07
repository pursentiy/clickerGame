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
        // ScreenBlocker убран из логики анимаций, чтобы не блокировать ввод
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
        
        // Эти поля используются ТОЛЬКО для активного перетаскивания (когда палец на экране)
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
            // Проверка _isDraggable гарантирует, что мы не возьмем две фигуры ОДНОВРЕМЕННО в руки,
            // но как только мы отпустим первую, флаг сбросится, и можно будет брать вторую.
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
            
            // Используем ID конкретной фигуры
            _puzzlesListWidget.TryShiftAllElements(figure.Id, false);
            
            // Анимации старта перетаскивания
            _draggingMenuScrollEmptyContainer.transform.DOScale(0, 0.3f).KillWith(this);
            _puzzlesListWidget.FadeDraggingContainerOverlay(true);
            _draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta(new Vector2(0, 0), 0.3f).KillWith(this);
        }
        
        private void OnDragEndFiguresSignal(IDraggable draggable, PointerEventData eventData)
        {
            var draggingFigureWidget = draggable?.GetAs<FigureMenuWidget>();
            
            // Проверки валидности
            if (draggingFigureWidget == null || _draggingFigure == null) 
                return;
            if (draggingFigureWidget.IsCompleted || draggingFigureWidget.Id != _draggingMenuScrollEmptyContainer.Id) 
                return;

            // 1. ЗАХВАТЫВАЕМ текущие объекты в локальные переменные для анимации
            var figureToAnimate = _draggingMenuScrollEmptyContainer;
            var visualObjectToAnimate = _draggingFigure;

            // 2. СРАЗУ СБРАСЫВАЕМ состояние драга, чтобы разрешить взятие следующей фигуры
            ClearDraggingFigureElements();
            SetDraggingEnabled(false);
            
            // 3. Разблокируем скролл (опционально, можно оставить заблокированным до конца анимации, но лучше разблокировать)
            _lockScrollAction?.SafeInvoke(false);

            try
            {
                var targets = _clickHandlerService.DetectFigureTarget(eventData, _figuresAssemblyCanvasRaycaster);
                var targetFigure = targets?.FirstOrDefault(f => f != null && f.Id == figureToAnimate.Id);

                if (targetFigure != null)
                {
                    // Передаем захваченные переменные
                    TryInsertMenuFigure(figureToAnimate, visualObjectToAnimate, targetFigure);
                }
                else
                {
                    // Передаем захваченные переменные
                    ResetDraggingFigure(figureToAnimate, visualObjectToAnimate);
                }
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(this, $"Failed to process drag end: {e.Message}");
                // В случае ошибки тоже пытаемся вернуть, используя захваченные переменные
                ResetDraggingFigure(figureToAnimate, visualObjectToAnimate);
            }
        }

        // Метод теперь принимает конкретные экземпляры, а не берет их из полей класса
        private void TryInsertMenuFigure(FigureMenuWidget figureWidget, GameObject visualObj, FigureTargetWidget target)
        {
            // УБРАН БЛОКИРОВЩИК ЭКРАНА, чтобы можно было взаимодействовать с другими элементами
            // var blockRef = _uiScreenBlocker.Block(15); 
            
            _soundHandler.PlaySound("success");

            // Запускаем анимации параллельно
            var animations = Promise.All(
                figureWidget.AnimateFigureConnection(),
                target.SetConnected(),
                _puzzlesListWidget.TryShiftAllElements(target.Id, false, true)
            );
            
            TrySetFigureConnectedSignal.Dispatch(target.Id);
            CheckLevelCompletionSignal.Dispatch();

            // Overlay убираем сразу, но проверяем, не начался ли новый драг
            // Если начался новый драг, FadeDraggingContainerOverlay(true) в OnBegin перебьет это.
            if (!_isDraggable)
            {
                 _puzzlesListWidget.FadeDraggingContainerOverlay(false);
            }
            
            animations
                .Then(FinalizeInsertion)
                .Then(CleanUp)
                .Catch(e => HandleError(e))
                .CancelWith(this);
            
            IPromise FinalizeInsertion()
            {
                figureWidget.SetFigureCompleted(true);
                target.SetFigureCompleted(true);
                // Уничтожаем конкретный визуальный объект
                return DestroyDraggingFigure(figureWidget, visualObj);
            }

            void CleanUp()
            {
                // Бамп холдера делаем, только если это уместно
                _puzzlesListWidget.BumpDraggingContainerHolder().CancelWith(this);
            }
        }
        
        // Метод теперь принимает конкретные экземпляры
        private void ResetDraggingFigure(FigureMenuWidget figureWidget, GameObject visualObj)
        {
            // УБРАН БЛОКИРОВЩИК, чтобы можно было брать другую фигуру пока эта летит
            // var blockRef = _uiScreenBlocker.Block(15);
            
            _soundHandler.PlaySound("fail");

            // Если начался новый драг, мы не должны выключать Overlay
            if (!_isDraggable)
            {
                _puzzlesListWidget.FadeDraggingContainerOverlay(false, 0.55f).CancelWith(this);
            }
            
            var animations = Promise.All(
                _puzzlesListWidget.TryShiftAllElements(figureWidget.Id, isInserting: true),
                // Анимируем полет конкретного объекта к конкретному виджету
                _puzzlesListWidget.AnimateMenuFigureFlightToPosition(figureWidget, visualObj)
            );

            animations
                .Then(FinalizeFigureReturn)
                .Then(() =>
                {
                    _puzzlesListWidget.BumpDraggingContainerHolder().CancelWith(this);
                })
                .Catch(HandleResetError)
                .CancelWith(this);
            
            IPromise FinalizeFigureReturn()
            {
                // Возвращаем конкретную фигуру
                _puzzlesListWidget.ReturnFigureBackToScroll(figureWidget.Id);
                figureWidget.SetFigureTransformPosition(Vector3.zero);
                
                // Важно: мы не вызываем ClearDraggingFigureElements() здесь, 
                // так как они уже могли быть заполнены НОВОЙ фигурой, которую игрок взял в руку.
                
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
            // Очищаем только ссылки на текущие перетаскиваемые объекты
            _draggingMenuScrollEmptyContainer = null;
            _draggingFigure = null;
        }
        
        // Модифицирован для удаления конкретного объекта
        private IPromise DestroyDraggingFigure(FigureMenuWidget figureWidget, GameObject visualObj)
        {
             // Удаляем из списка
             if (_puzzlesListWidget.DestroyFigure(figureWidget.Id))
                 _puzzlesListWidget.RefreshBasePositionsAfterRemoval();
             
             // Удаляем физический объект (если он еще существует)
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
            // Это работает только пока палец держит фигуру.
            // Как только сработал OnDragEnd, мы обнулили _draggingFigure,
            // и этот апдейт перестал работать. Далее фигуру двигает DOTween в ResetDraggingFigure.
            if (_draggingMenuScrollEmptyContainer == null || _draggingFigure == null || !_isDraggable)
            {
                return;
            }

            _draggingFigure.transform.position = Input.mousePosition;
        }
    }
}