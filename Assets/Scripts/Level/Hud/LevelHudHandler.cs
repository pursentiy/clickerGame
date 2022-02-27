using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Figures.Animals;
using Handlers;
using Installers;
using Plugins.FSignal;
using RSG;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using ProgressHandler = Handlers.ProgressHandler;

namespace Level.Hud
{
    public class LevelHudHandler : InjectableMonoBehaviour, ILevelHudHandler
    {
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PopupHandler _popupHandler;
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private SoundHandler _soundHandler;

        [SerializeField] private RectTransform _figuresParentTransform;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private HorizontalLayoutGroup _figuresGroup;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private List<FigureMenu> _figureAnimalsMenuList;
        private float _figuresGroupSpacing;
        private Sequence _shiftingSequence;

        protected override void Awake()
        {
            _figureAnimalsMenuList = new List<FigureMenu>();
            
            _backButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                GoToMainMenuScreen();
            });
            _settingsButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _popupHandler.ShowSettings();
            });

            _figuresGroupSpacing = _figuresGroup.spacing;
        }

        public FSignal BackToMenuClickSignal { get; } = new FSignal();

        public void SetupScrollMenu(List<LevelFigureParams> levelFiguresParams)
        {
            levelFiguresParams.ForEach(SetFigure);
        }
        
        public void SetInteractivity(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
        }

        private void SetFigure(LevelFigureParams figureParams)
        {
            var figurePrefab = _figuresStorageData.GetMenuFigure(_progressHandler.CurrentPackNumber,
                _progressHandler.CurrentLevelNumber, figureParams.FigureId);

            if (figurePrefab == null)
            {
                Debug.LogWarning($"Could not find figure with type {figureParams.FigureId} in {this}");
                return;
            }

            if (figureParams.Completed)
            {
                return;
            }
            
            var figure = Instantiate(figurePrefab, _figuresParentTransform);
            figure.SetUpDefaultParamsFigure(figureParams.FigureId);
            figure.SetScale(1);
            _figureAnimalsMenuList.Add(figure);
            
            SetupDraggingSignalsHandlers(figure);
        }

        private void SetupDraggingSignalsHandlers(FigureMenu figure)
        {
            figure.OnBeginDragSignal.AddListener(OnBeginDragSignalHandler);
            figure.OnDraggingSignal.AddListener(OnDraggingSignalHandler);
            figure.OnEndDragSignal.AddListener(OnEndDragSignalHandler);
        }

        private void OnBeginDragSignalHandler(PointerEventData eventData)
        {
            _scrollRect.SendMessage("OnBeginDrag", eventData);
        }
        
        private void OnDraggingSignalHandler(PointerEventData eventData)
        {
            _scrollRect.SendMessage("OnDrag", eventData);
        }
        
        private void OnEndDragSignalHandler(PointerEventData eventData)
        {
            _scrollRect.SendMessage("OnEndDrag", eventData);
        }

        private void GoToMainMenuScreen()
        {
            _screenHandler.ShowChooseLevelScreen(BackToMenuClickSignal);
        }

        public void ShiftAllElements(bool isInserting, int figureId, Promise animationPromise)
        {
            if (_figureAnimalsMenuList.Count == 0)
            {
                animationPromise.Resolve();
                return;
            }
            
            if(_shiftingSequence != null && _shiftingSequence.IsActive())
                _shiftingSequence.Complete();

            _shiftingSequence = DOTween.Sequence();
            _figuresGroup.enabled = false;

            _figureAnimalsMenuList.ForEach(figure =>
            {
                if (figure.FigureId <= figureId || figure == null)
                    return;

                var position = figure.ContainerTransform.localPosition;
                _shiftingSequence.Join(isInserting
                    ? figure.ContainerTransform.DOLocalMove(new Vector2(position.x + figure.InitialWidth, position.y), 0.25f)
                    : figure.ContainerTransform.DOLocalMove(new Vector2(position.x - figure.InitialWidth, position.y), 0.25f));
            });

            _shiftingSequence.OnComplete(animationPromise.Resolve);
        }

        public void TryShiftAllElementsAfterRemoving(int figureId, Promise animationPromise)
        {
            if (_figureAnimalsMenuList.Count <= 1)
            {
                animationPromise.Resolve();
                return;
            }
            
            var shiftingSequence = DOTween.Sequence();
            _figureAnimalsMenuList.ForEach(figure =>
            {
                if (figure.FigureId <= figureId)
                    return;
                
                var position = figure.ContainerTransform.localPosition;
                shiftingSequence.Join(figure.ContainerTransform.DOLocalMove(new Vector2(position.x - _figuresGroupSpacing, position.y), 0.15f));
            });
            
            shiftingSequence.OnComplete(animationPromise.Resolve);
        }

        public void DestroyFigure(int figureId)
        { 
            _figureAnimalsMenuList.FirstOrDefault(figure => figure.FigureId == figureId)?.Destroy();
            _figureAnimalsMenuList = _figureAnimalsMenuList.Where(figure => figure.FigureId != figureId).ToList();
        }

        public void LockScroll(bool value)
        {
            _scrollRect.horizontal = !value;
        }

        public FigureMenu GetFigureById(int figureId)
        {
            return _figureAnimalsMenuList.FirstOrDefault(figure => figure.FigureId == figureId);
        }

        public List<FSignal<FigureMenu>> GetOnBeginDragFiguresSignal()
        {
            return _figureAnimalsMenuList.Select(figure => figure.OnBeginDragFigureSignal).ToList();
        }

        public List<FSignal<PointerEventData>> GetOnDragEndFiguresSignal()
        {
            return _figureAnimalsMenuList.Select(figure => figure.OnEndDragSignal).ToList();
        }

        public void ReturnFigureBackToScroll(int figureId)
        {
            var figure = GetFigureById(figureId);
            figure.FigureTransform.SetParent(figure.ContainerTransform);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();

            UnsubscribeFromDraggingSignals();
        }

        private void UnsubscribeFromDraggingSignals()
        {
            _figureAnimalsMenuList.ForEach(figure =>
            {
                figure.OnBeginDragSignal.RemoveListener(OnBeginDragSignalHandler);
                figure.OnDraggingSignal.RemoveListener(OnDraggingSignalHandler);
                figure.OnEndDragSignal.RemoveListener(OnEndDragSignalHandler);
            });
        }
    }
}