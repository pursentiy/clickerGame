using System.Collections.Generic;
using System.Linq;
using Common.Handlers.Draggable;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Installers;
using Level.Widgets;
using Plugins.FSignal;
using RSG;
using Services;
using Services.CoroutineServices;
using Storage;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Widgets.Puzzles
{
    public class PuzzlesListWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly CoroutineService _coroutineService;
        
        [SerializeField] private RectTransform _figuresDraggingContainer;
        [SerializeField] private RectTransform _figuresAssemblyContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private HorizontalLayoutGroup _figuresLayoutGroup;
        
        private List<FigureMenuWidget> _figuresMenuList = new List<FigureMenuWidget>();
        private List<FigureTargetWidget> _figuresTargetList = new List<FigureTargetWidget>();
        private Sequence _shiftingSequence;
        

        public IReadOnlyList<FSignal<IDraggable, PointerEventData>> OnBeginDragFiguresSignals => 
            _figuresMenuList.Select(figure => figure.OnBeginDragSignal).ToList();
        public IReadOnlyList<FSignal<IDraggable, PointerEventData>> OnDragEndFiguresSignals => 
            _figuresMenuList.Select(figure => figure.OnEndDragSignal).ToList();
        
        public FigureMenuWidget GetMenuFigureById(int figureId)
        {
            return _figuresMenuList.FirstOrDefault(figure => figure.Id == figureId);
        }
        
        public void Initialize(List<TargetFigureInfo> figuresTargetList, List<MenuFigureInfo> figuresMenuList)
        {
            if (figuresMenuList.IsCollectionNullOrEmpty() || figuresTargetList.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"{nameof(List<FigureTargetWidget>)} or {nameof(List<FigureMenuWidget>)} lists are null or empty");
                return;
            }

            if (figuresMenuList.Count != figuresTargetList.Count)
            {
                LoggerService.LogError(this, $"Items count {figuresTargetList.Count} in {nameof(List<TargetFigureInfo>)} does not equal count {figuresTargetList.Count} in {nameof(List<MenuFigureInfo>)} at {nameof(Initialize)}");
                return;
            }
            
            figuresMenuList.ForEach(InitializeMenuFigure);
            figuresTargetList.ForEach(InitializeTargetFigure);
        }

        public void FinishAllShiftingAnimations()
        {
            _shiftingSequence?.Kill(true);
        }
        
        public IPromise TryShiftAllElements(int figureId, bool isInserting)
        {
            if (_figuresMenuList.Count <= 0)
                return Promise.Resolved();

            FinishAllShiftingAnimations();
            _shiftingSequence = DOTween.Sequence().KillWith(this);
            
            var targetFigures = _figuresMenuList
                .Where(f => f != null && f.Id > figureId)
                .OrderBy(f => f.Id) // Важно для корректного Stagger эффекта
                .ToList();

            for (var i = 0; i < targetFigures.Count; i++)
            {
                var figure = targetFigures[i];
                var position = figure.ContainerTransform.localPosition;
        
                var finalPosition = isInserting
                    ? new Vector2(position.x + figure.InitialWidth, position.y)
                    : new Vector2(position.x - figure.InitialWidth, position.y);
                
                var delay = i * 0.01f; 

                _shiftingSequence.Join(
                    figure.ContainerTransform
                        .DOLocalMove(finalPosition, 0.4f) // Увеличили длительность
                        .SetEase(Ease.OutBack, 0.8f)      // Добавили мягкий отскок
                        .SetDelay(delay)                  // Добавили эффект волны
                );
                
                _shiftingSequence.Join(
                    figure.ContainerTransform
                        .DOPunchScale(new Vector3(0.05f, -0.05f, 0), 0.3f, 5, 1f)
                        .SetDelay(delay)
                );
            }
    
            return _shiftingSequence.AsPromise();
        }
        
        public bool DestroyFigure(int figureId)
        { 
            var figure = _figuresMenuList.FirstOrDefault(figure => figure.Id == figureId);
            if (figure == null) 
                return false;
            
            _figuresMenuList.Remove(figure);
            figure.DestroyWidget();
            return true;
        }
        
        public IPromise AnimateMenuFigureFlightToPosition(FigureMenuWidget draggingMenuScrollEmptyContainer, GameObject draggingFigure)
        {
            // var sequence = DOTween.Sequence()
            //     .Append(draggingFigure.transform.DOMove(draggingMenuScrollEmptyContainer.InitialPosition, 0.4f))
            //     .Join(draggingMenuScrollEmptyContainer.transform.DOScale(1, 0.3f))
            //     .Join(draggingMenuScrollEmptyContainer.ContainerTransform.DOSizeDelta
            //         (new Vector2(draggingMenuScrollEmptyContainer.InitialWidth, draggingMenuScrollEmptyContainer.InitialHeight), 0.3f))
            //     .KillWith(this);
            
            var sequence = DOTween.Sequence().KillWith(this);
            
            sequence.Append(draggingFigure.transform
                .DOMove(draggingMenuScrollEmptyContainer.InitialPosition, 0.5f)
                .SetEase(Ease.OutBack, 1.2f)); // 1.2f — это амплитуда "отскока"

            sequence.Join(draggingMenuScrollEmptyContainer.transform.DOScale(1, 0.3f));
            sequence.Join(draggingMenuScrollEmptyContainer.ContainerTransform
                .DOSizeDelta(new Vector2(draggingMenuScrollEmptyContainer.InitialWidth, draggingMenuScrollEmptyContainer.InitialHeight), 0.3f)
                .SetEase(Ease.OutQuad));
            
            // sequence.Append(draggingMenuScrollEmptyContainer.transform
            //     .DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.4f, 5, 1f));

           return sequence.AsPromise();
        }
        
        public IPromise ReturnFigureBackToScroll(int figureId)
        {
            var figure = GetMenuFigureById(figureId);
            if (figure == null)
            {
                LoggerService.LogError(this, $"[{nameof(ReturnFigureBackToScroll)}]: No figure found with id " + figureId);
                return Promise.Resolved();
            }
            
            figure.FigureTransform.SetParent(figure.ContainerTransform);
            figure.FigureTransform.offsetMin = Vector2.zero;
            figure.FigureTransform.offsetMax = Vector2.zero;
            
            return Promise.Resolved();
        }
        
        private void InitializeMenuFigure(MenuFigureInfo menuFigureInfo)
        {
            if (menuFigureInfo == null)
            {
                LoggerService.LogError(this, $"{nameof(MenuFigureInfo)} is null at {nameof(InitializeMenuFigure)}");
                return;
            }
            
            var menuFigure = Instantiate(menuFigureInfo.Widget, _figuresDraggingContainer);
            menuFigure.Initialize(menuFigureInfo.Id);
            menuFigure.SaveInitialWidthAndHeight();

            _figuresMenuList.Add(menuFigure);
        }
        
        private void InitializeTargetFigure(TargetFigureInfo figureTargetInfo)
        {
            if (figureTargetInfo == null)
            {
                LoggerService.LogError(this, $"{nameof(TargetFigureInfo)} is null at {nameof(InitializeTargetFigure)}");
                return;
            }

            var targetFigure = Instantiate(figureTargetInfo.Widget, _figuresAssemblyContainer);
            targetFigure.Initialize(figureTargetInfo.Id);
            _figuresTargetList.Add(targetFigure);
        }
    }
}