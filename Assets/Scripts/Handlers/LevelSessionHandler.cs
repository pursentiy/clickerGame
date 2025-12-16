using System.Collections;
using System.Linq;
using DG.Tweening;
using Figures.Animals;
using Installers;
using Level.Click;
using Level.Game;
using Level.Hud;
using Level.Widgets;
using RSG;
using Services;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Disposable;
using Zenject;

namespace Handlers
{
    public class LevelSessionHandler : InjectableMonoBehaviour, ILevelSessionHandler
    {
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LevelInfoTrackerService _levelInfoTrackerService;

        [SerializeField] private RectTransform _gameMainCanvasTransform;
        [SerializeField] private RectTransform _draggingTransform;
        [SerializeField] private ClickHandler _clickHandler;
        [SerializeField] private ParticleSystem _finishedFigureParticles;

        private LevelVisualHandler _levelVisualHandler;
        private LevelHudHandler _levelHudHandler;
        private FigureMenu _draggingFigureContainer;
        private GameObject _draggingFigureImage;
        private FigureMenu _menuFigure;
        private bool _isDraggable;
        private LevelParams _currentLevelParams;

        private Sequence _resetDraggingAnimationSequence;
        private Sequence _completeDraggingAnimationSequence;
        
        private bool IsLevelComplete => _currentLevelParams != null && 
                                        _currentLevelParams.LevelFiguresParamsList.TrueForAll(levelFigureParams => levelFigureParams.Completed);

        public void StartLevel(LevelParams levelParams, LevelHudHandler levelHudHandler, Color defaultColor)
        {
            if (levelParams == null)
            {
                Debug.LogError("LevelParams cannot be null");
                return;
            }
            _currentLevelParams =  levelParams;
            var levelId = $"{_playerProgressService.CurrentPackNumber}-{_playerProgressService.CurrentLevelNumber}";
            _levelInfoTrackerService.StartLevelTracking(levelId);
            
            SetupClickHandler();
            SetupHud(levelParams, levelHudHandler);
            SetupLevelScreenHandler(levelParams, defaultColor);
            
            TryHandleLevelCompletion(true);
            _soundHandler.PlaySound("start");
        }

        private void TryHandleLevelCompletion(bool onEnter)
        {
            if (!_playerProgressService.CheckForLevelCompletion(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber))
            {
                return;
            }
            
            _levelInfoTrackerService.StopLevelTracking();
            _levelHudHandler.SetInteractivity(false);
            StartCoroutine(AwaitFinishLevel(onEnter));
        }

        private IEnumerator AwaitFinishLevel(bool onEnter)
        {
            yield return new WaitForSeconds(_screenHandler.AwaitChangeScreenTime);
            _soundHandler.PlaySound("finished");
            _screenHandler.ShowLevelCompleteScreen(onEnter, ResetLevel, 
                _figuresStorageData.GetLevelParamsData(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber).LevelImage, _levelVisualHandler.ScreenColorAnimation.Gradient);
        }

        private void SetupClickHandler()
        {
            _clickHandler.enabled = true;
        }

        private void SetupLevelScreenHandler(LevelParams packParam, Color defaultColor)
        {
            _levelVisualHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelVisualHandler>(_figuresStorageData.GetLevelVisualHandler(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber));
            _levelVisualHandler.SetupLevel(packParam.LevelFiguresParamsList, defaultColor);
        }

        private void SetupHud(LevelParams packParam, LevelHudHandler levelHudHandler)
        {
            _levelHudHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelHudHandler>(levelHudHandler, _gameMainCanvasTransform);
            _levelHudHandler.SetupScrollMenu(packParam.LevelFiguresParamsList);
            
            _levelHudHandler.BackToMenuClickSignal.AddListener(ResetLevel);
            _levelHudHandler.Initialize(packParam.LevelBeatingTimeInfo);
            _levelHudHandler.GetOnBeginDragFiguresSignal().ForEach(signal => { signal.AddListener(StartElementDragging); });
            _levelHudHandler.GetOnDragEndFiguresSignal().ForEach(signal => { signal.AddListener(EndElementDragging); });
        }

        private void StartElementDragging(FigureMenu figure)
        {
            if (_isDraggable || figure == null || figure.IsCompleted)
            {
                return;
            }

            _isDraggable = true;
            _menuFigure = figure;
            _levelHudHandler.LockScroll(true);
            
            SetupDraggingFigure(figure.FigureId);
        }

        private void SetupDraggingFigure(int figureId)
        {
            _draggingFigureContainer = _levelHudHandler.GetFigureById(figureId);
            _draggingFigureImage = _draggingFigureContainer.FigureTransform.gameObject;
            _draggingFigureContainer.InitialPosition = _draggingFigureContainer.transform.position;
            _draggingFigureContainer.FigureTransform.SetParent(_draggingTransform);
            
            _levelHudHandler.ShiftAllElements(false, figureId, new Promise());
            _draggingFigureContainer.transform.DOScale(0, 0.3f);
            _draggingFigureContainer.ContainerTransform.DOSizeDelta(new Vector2(0, 0), 0.3f);
        }

        private void EndElementDragging(PointerEventData eventData)
        {
            var releasedOnFigure = _clickHandler.TryGetFigureAnimalTargetOnDragEnd(eventData);

            if (_draggingFigureContainer == null)
            {
                return;
            }

            if (releasedOnFigure == null)
            {
                ResetDraggingFigure();
                return;
            }

            if (_draggingFigureContainer.FigureId == releasedOnFigure.FigureId)
            {
                _soundHandler.PlaySound("success");
                var shiftingAnimationPromise = new Promise();
                _levelHudHandler.TryShiftAllElementsAfterRemoving(_draggingFigureContainer.FigureId, shiftingAnimationPromise);
                
                TrySetFigureInserted(releasedOnFigure.FigureId);
                
                _completeDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigureImage.transform.DOScale(0, 0.4f))
                    .InsertCallback(0.25f, PlayFinishParticles).KillWith(this);

                shiftingAnimationPromise.Then(() =>
                {
                    _completeDraggingAnimationSequence.OnComplete(() =>
                        {
                            releasedOnFigure.SetConnected();
                            SetMenuFigureConnected();
                            TryHandleLevelCompletion(false);
                        });
                });
            }
            else
            {
                ResetDraggingFigure();
            }
        }

        private void PlayFinishParticles()
        {
            _finishedFigureParticles.transform.position = _draggingFigureImage.transform.position;
            _finishedFigureParticles.Simulate(0);
            _finishedFigureParticles.Play();
        }

        private void SetMenuFigureConnected()
        {
            var menuFigurePromise = new Promise();
            _menuFigure.SetConnected(menuFigurePromise);
            menuFigurePromise.Then(() =>
            {
                _levelHudHandler.DestroyFigure(_draggingFigureContainer.FigureId);
                ClearDraggingFigure();
            });
        }

        private void ResetDraggingFigure()
        {
            _soundHandler.PlaySound("fail");
            var shiftingAnimationPromise = new Promise();
            _levelHudHandler.ShiftAllElements(true, _draggingFigureContainer.FigureId, shiftingAnimationPromise);

            _resetDraggingAnimationSequence = DOTween.Sequence()
                .Append(_draggingFigureImage.transform.DOMove(_draggingFigureContainer.transform.position, 0.4f))
                .Join(_draggingFigureContainer.transform.DOScale(1, 0.3f))
                .Join(_draggingFigureContainer.ContainerTransform.DOSizeDelta
                    (new Vector2(_draggingFigureContainer.InitialWidth, _draggingFigureContainer.InitialHeight), 0.3f));
            
            shiftingAnimationPromise.Then(() =>
            {
                _resetDraggingAnimationSequence.OnComplete(() =>
                    {
                        _levelHudHandler.ReturnFigureBackToScroll(_draggingFigureContainer.FigureId);
                        _draggingFigureContainer.FigureTransform.transform.localPosition = Vector3.zero;
                        ClearDraggingFigure();
                    });
            });
        }

        private void ClearDraggingFigure()
        {
            _isDraggable = false;
            _levelHudHandler.LockScroll(false);
            _draggingFigureContainer = null;
            _draggingFigureImage = null;
        }

        private void Update()
        {
            TryUpdateDraggingFigurePosition();
        }

        private void TryUpdateDraggingFigurePosition()
        {
            if (_draggingFigureContainer == null || _draggingFigureImage == null || !_isDraggable)
            {
                return;
            }

            _draggingFigureImage.transform.position = Input.mousePosition;
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
        
        private void TrySetFigureInserted(int figureId)
        {
            if (_currentLevelParams == null)
            {
                return;
            }

            var levelFigure = _currentLevelParams.LevelFiguresParamsList.FirstOrDefault(level => level.FigureId == figureId);
            
            if (levelFigure == null)
            {
                Debug.LogWarning($"Could not update progress with figure id {figureId} in {this}");
                return;
            }
            
            levelFigure.Completed = true;
        }
    }
}
