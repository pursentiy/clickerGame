using DG.Tweening;
using Figures;
using Figures.Animals;
using Installers;
using Level.Click;
using Level.Game;
using Level.Hud;
using Storage.Levels.Params;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Handlers
{
    public class LevelSessionHandler : InjectableMonoBehaviour, ILevelSessionHandler
    {
        [Inject] private ProgressHandler _progressHandler;
        [Inject] private ScreenHandler _screenHandler;

        [SerializeField] private RectTransform _gameMainCanvasTransform;
        [SerializeField] private RectTransform _draggingTransform;
        [SerializeField] private ClickHandler _clickHandler;

        private LevelVisualHandler _levelVisualHandler;
        private LevelHudHandler _levelHudHandler;
        private FigureAnimalsMenu _draggingFigure;
        private FigureAnimalsMenu _menuFigure;
        private bool _isDraggable;

        private Sequence _resetDraggingAnimationSequence;
        private Sequence _completeDraggingAnimationSequence;

        public void StartLevel(LevelParams packParams, LevelHudHandler levelHudHandler, Color defaultColor)
        {
            SetupClickHandler();
            SetupHud(packParams, levelHudHandler);
            SetupFigures(packParams, defaultColor);
            
            TryHandleLevelCompletion();
        }

        private void TryHandleLevelCompletion()
        {
            if (!_progressHandler.CheckForLevelCompletion(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber))
            {
                return;
            }
            
            _screenHandler.ShowLevelCompleteScreen(_levelVisualHandler.TextureCamera, ResetLevel);
        }

        private void SetupClickHandler()
        {
            _clickHandler.enabled = true;
        }

        private void SetupFigures(LevelParams packParam, Color defaultColor)
        {
            _levelVisualHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelVisualHandler>(packParam.LevelVisualHandler);
            _levelVisualHandler.SetupLevel(packParam.LevelFiguresParamsList, defaultColor);
        }

        private void SetupHud(LevelParams packParam, LevelHudHandler levelHudHandler)
        {
            _levelHudHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelHudHandler>(levelHudHandler, _gameMainCanvasTransform);
            _levelHudHandler.SetupScrollMenu(packParam.LevelFiguresParamsList);
            
            _levelHudHandler.BackToMenuClickSignal.AddListener(ResetLevel);
            _levelHudHandler.GetOnBeginDragFiguresSignal().ForEach(signal => { signal.AddListener(StartElementDragging); });
            _levelHudHandler.GetOnDragEndFiguresSignal().ForEach(signal => { signal.AddListener(EndElementDragging); });
        }

        private void StartElementDragging(FigureAnimalsMenu figure)
        {
            if (_isDraggable || figure == null || figure.IsCompleted)
            {
                return;
            }

            _isDraggable = true;
            _menuFigure = figure;
            _levelHudHandler.LockScroll(true);
            
            SetupDraggingFigure(figure.FigureType);
        }

        private void SetupDraggingFigure(FigureType figureType)
        {
            _draggingFigure = _levelHudHandler.GetFigureByType(figureType);
            _draggingFigure.InitialPosition = _draggingFigure.transform.position;
            _draggingFigure.SiblingPosition = _draggingFigure.transform.GetSiblingIndex();
            _draggingFigure.transform.SetParent(_draggingTransform);
        }

        private void EndElementDragging(PointerEventData eventData)
        {
            var releasedOnFigure = _clickHandler.TryGetFigureAnimalTargetOnDragEnd(eventData);

            if (_draggingFigure == null)
            {
                return;
            }

            if (releasedOnFigure == null)
            {
                ResetDraggingFigure();
                return;
            }

            if (_draggingFigure.FigureType == releasedOnFigure.FigureType)
            {
                _progressHandler.UpdateProgress(_progressHandler.CurrentPackNumber, _progressHandler.CurrentLevelNumber, releasedOnFigure.FigureType);
                _completeDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigure.transform.DOScale(0, 0.4f)).AppendCallback(() =>
                {
                    ClearDraggingFigure(true);
                    releasedOnFigure.SetConnected();
                    _menuFigure.SetConnected();
                    TryHandleLevelCompletion();
                });
            }
            else
            {
                ResetDraggingFigure();
            }
        }

        private void ResetDraggingFigure()
        {
            _resetDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigure.transform.DOMove(_menuFigure.InitialPosition, 0.4f))
                .OnComplete(() =>
                {
                    _levelHudHandler.ReturnFigureBackToScroll(_draggingFigure.FigureType);
                    ClearDraggingFigure(false);
                });
        }

        private void ClearDraggingFigure(bool removeFigure)
        {
            _isDraggable = false;
            _levelHudHandler.LockScroll(false);
            
            if (removeFigure)
            {
                _draggingFigure.PoolObject.ResetObject();
            }
            
            _draggingFigure = null;
        }

        private void Update()
        {
            TryUpdateDraggingFigurePosition();
        }

        private void TryUpdateDraggingFigurePosition()
        {
            if (_draggingFigure == null || !_isDraggable)
            {
                return;
            }

            _draggingFigure.transform.position = Input.mousePosition;
        }

        private void ResetLevel()
        {
            if (_levelHudHandler != null)
            {
                _levelHudHandler.GetOnBeginDragFiguresSignal().ForEach(signal => { signal.RemoveListener(StartElementDragging); });
                _levelHudHandler.GetOnDragEndFiguresSignal().ForEach(signal => { signal.RemoveListener(EndElementDragging); });
            }

            ResetAnimationSequences();
            DestroyHandlers();

            if (_clickHandler != null)
            {
                _clickHandler.enabled = false;
            }
        }

        private void DestroyHandlers()
        {
            if (_levelHudHandler != null)
            {
                _levelHudHandler.BackToMenuClickSignal.RemoveListener(ResetLevel);
                Destroy(_levelHudHandler.gameObject);
                _levelHudHandler = null;
            }
            
            if (_levelVisualHandler != null)
            {
                Destroy(_levelVisualHandler.gameObject);
                _levelVisualHandler = null;
            }
        }

        private void ResetAnimationSequences()
        {
            _resetDraggingAnimationSequence?.Kill();
            _completeDraggingAnimationSequence?.Kill();
        }
    }
}
