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
        
        [SerializeField] private RectTransform _figuresDraggingContainerHolder;
        [SerializeField] private RectTransform _figuresDraggingContainer;
        [SerializeField] private RectTransform _figuresAssemblyContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private HorizontalLayoutGroup _figuresLayoutGroup;
        [SerializeField] private CanvasGroup _disabledDraggingContainerOverlay;
        
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

            // 1. Принудительно обновляем лейаут, чтобы Unity расставила элементы
            Canvas.ForceUpdateCanvases();
            _figuresLayoutGroup.CalculateLayoutInputHorizontal();
            _figuresLayoutGroup.SetLayoutHorizontal();
    
            // 2. Сохраняем "родные" позиции для каждого элемента
            foreach (var figure in _figuresMenuList)
            {
                figure.SaveBaseLocalPosition();
            }

            // 3. Отключаем LayoutGroup, чтобы он не мешал DOTween'у
            _figuresLayoutGroup.enabled = false;

            FadeDraggingContainerOverlay(false, fast: true);
        }

        public IPromise BumpDraggingContainerHolder()
        {
            return Promise.Resolved();
            _figuresDraggingContainerHolder.DOKill(true);

            var sequence = DOTween.Sequence().KillWith(_figuresDraggingContainerHolder.gameObject);

            var force = 0.015f;
            sequence.Append(_figuresDraggingContainerHolder.transform
                .DOPunchScale(new Vector3(force, force, force), 0.2f, 3, 0.7f)
                .SetEase(Ease.OutQuad));
            
            sequence.Join(_figuresDraggingContainerHolder.transform
                .DOPunchRotation(new Vector3(0, 0, 0.05f), 0.3f, 2, 0.5f)
                .SetEase(Ease.OutSine));

            return sequence.AsPromise();
        }

        public IPromise FadeDraggingContainerOverlay(bool isVisible, float duration = 0.3f, bool fast = false)
        {
            _disabledDraggingContainerOverlay.DOKill();

            var targetAlpha = isVisible ? 0.5f : 0f;
            var easeType = isVisible ? Ease.OutQuad : Ease.InQuad;

            if (isVisible)
                _disabledDraggingContainerOverlay.blocksRaycasts = true;

            var tweener = _disabledDraggingContainerOverlay
                .DOFade(targetAlpha, duration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    if (!isVisible) _disabledDraggingContainerOverlay.blocksRaycasts = false;
                })
                .KillWith(this);

            if (fast)
            {
                tweener.Kill(true);
            }

            return tweener.AsPromise();
        }

        public IPromise TryShiftAllElements(int figureId, bool isInserting, bool shiftByPadding = false)
        {
            if (_figuresMenuList.Count <= 0)
                return Promise.Resolved();

            _shiftingSequence?.Kill(true);
            _shiftingSequence = DOTween.Sequence().KillWith(this);

            // Берем все фигуры, которые находятся справа от той, которую мы взяли
            var targetFigures = _figuresMenuList
                .Where(f => f != null && f.Id > figureId)
                .OrderBy(f => f.Id)
                .ToList();

            for (var i = 0; i < targetFigures.Count; i++)
            {
                var figure = targetFigures[i];

                // ВАЖНО: Расчет оффсета. Обычно нужно учитывать и Spacing лейаута
                var spacing = _figuresLayoutGroup.spacing;
                var moveDistance = shiftByPadding
                    ? spacing
                    : (figure.InitialWidth + spacing); // Сдвигаем на ширину + отступ

                // Логика:
                // Если isInserting (возврат/вставка) -> возвращаем на Базовую позицию.
                // Если !isInserting (взяли фигуру) -> сдвигаем влево от Базовой позиции.

                Vector3 targetPosition;

                if (isInserting)
                {
                    // Возвращаем все как было (на базовые места)
                    targetPosition = figure.BaseLocalPosition;
                }
                else
                {
                    // Сдвигаем влево, чтобы закрыть пустоту
                    // (Предполагаем горизонтальный скролл слева направо)
                    targetPosition = figure.BaseLocalPosition - new Vector3(moveDistance, 0, 0);
                }

                var delay = i * 0.02f; // Чуть увеличил задержку для красоты

                _shiftingSequence.Join(
                    figure.ContainerTransform
                        .DOLocalMove(targetPosition, 0.4f)
                        .SetEase(Ease.OutBack, 0.8f)
                        .SetDelay(delay)
                );

                // Панч эффект оставляем без изменений
                _shiftingSequence.Join(
                    figure.ContainerTransform
                        .DOPunchScale(new Vector3(0.05f, -0.05f, 0), 0.3f, 5, 1f)
                        .SetDelay(delay)
                );
            }

            return _shiftingSequence.AsPromise();
        }

        public void RefreshBasePositionsAfterRemoval()
        {
            // Вариант А: Если мы просто хотим зафиксировать текущее положение как новое базовое
            // (Использовать, только если анимация сдвига завершилась полностью)
            /* foreach (var figure in _figuresMenuList)
            {
                figure.SaveBaseLocalPosition();
            }
            */

            // Вариант Б (Надежный): Пересчитать математически
            // Так как LayoutGroup отключен, мы можем сами рассчитать, где они должны стоять.
            // Это предотвратит накопление ошибок float.

            float currentX = _figuresLayoutGroup.padding.left;
            float spacing = _figuresLayoutGroup.spacing;

            // Сортируем по порядку отображения (предполагаем по ID или индексу в списке)
            var sortedFigures = _figuresMenuList.OrderBy(f => f.Id).ToList();

            foreach (var figure in sortedFigures)
            {
                // Устанавливаем новую базу
                var newBasePos = new Vector3(currentX + figure.InitialWidth / 2f, figure.BaseLocalPosition.y, 0);
                // Примечание: pivot у элементов может влиять на то, нужно ли прибавлять половину ширины.
                // Если Pivot (0, 0.5) -> просто currentX. Если (0.5, 0.5) -> currentX + width/2.
                // Проще всего взять текущую Y и Z, а X высчитать.

                // Но самый простой способ, если у вас уже сработал Shift влево:
                // Просто обновить базу на текущую позицию, к которой они приехали.
                figure.SaveBaseLocalPosition();

                currentX += figure.InitialWidth + spacing;
            }
        }
        
        public bool DestroyFigure(int figureId)
        { 
            var figure = _figuresMenuList.FirstOrDefault(figure => figure.Id == figureId);
            if (figure == null) 
                return false;
    
            _figuresMenuList.Remove(figure);
            figure.DestroyWidget();
    
            // Фигуры уже сдвинулись влево анимацией TryShiftAllElements(..., false).
            // Теперь это их новые законные места.
            foreach (var f in _figuresMenuList)
            {
                // Если анимация еще идет, это может быть опасно, поэтому лучше
                // вызывать это обновление только после завершения анимации вставки.
                // Но для надежности лучше пересчитать BaseLocalPosition на основе целевой точки сдвига.
        
                // Самый надежный фикс для "после удаления":
                // Вычесть (Width + Spacing) из BaseLocalPosition всех фигур справа от удаленной.
                if (f.Id > figureId)
                {
                    var shift = f.InitialWidth + _figuresLayoutGroup.spacing;
                    f.UpdateBaseLocalPosition(f.BaseLocalPosition - new Vector3(shift, 0, 0));
                }
            }
    
            return true;
        }
        
        public IPromise AnimateMenuFigureFlightToPosition(FigureMenuWidget draggingMenuScrollEmptyContainer, GameObject draggingFigure)
        {
            var sequence = DOTween.Sequence().KillWith(this);
            
            sequence.Append(draggingFigure.transform
                .DOMove(draggingMenuScrollEmptyContainer.InitialPosition, 0.5f)
                .SetEase(Ease.OutBack, 1.2f));
            
            sequence.Join(draggingMenuScrollEmptyContainer.transform.DOScale(1, 0.3f));
            
            sequence.Join(draggingMenuScrollEmptyContainer.ContainerTransform
                .DOSizeDelta(new Vector2(draggingMenuScrollEmptyContainer.InitialWidth, draggingMenuScrollEmptyContainer.InitialHeight), 0.3f)
                .SetEase(Ease.OutQuad));
            sequence.Insert(0.4f,draggingFigure.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.4f, 5, 1f));

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