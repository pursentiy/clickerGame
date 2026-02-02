using System;
using System.Collections;
using System.Linq;
using Common.Data.Info;
using Components.Levels.Figures;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using Level.FinishLevelSequence;
using Level.Hud;
using Level.Widgets;
using RSG;
using Services;
using Services.CoroutineServices;
using Storage.Snapshots.LevelParams;
using UI.Popups.CompleteLevelInfoPopup;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace Components.Levels
{
    public class LevelSessionHandler : MonoBehaviour, IDisposableHandlers
    {
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly LevelHelperService _levelHelperService;
        [Inject] private readonly ClickHandlerService _clickHandlerService;
        [Inject] private readonly UIBlockHandler _uiBlockHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly CoroutineService _coroutineService;

        [SerializeField] private RectTransform _gameMainCanvasTransform;
        [SerializeField] private RectTransform _draggingTransform;

        private LevelHudHandler _levelHudHandler;
        private FigureMenu _draggingFigureContainer;
        private GameObject _draggingFigureImage;
        private FigureMenu _menuFigure;
        private bool _isDraggable;
        private LevelParamsSnapshot _currentLevelParams;

        private Sequence _resetDraggingAnimationSequence;
        private Sequence _completeDraggingAnimationSequence;
        private Coroutine _finishCoroutine;
        private PackInfo _packInfo;
        
        private bool IsLevelComplete => _currentLevelParams != null && 
                                        _currentLevelParams.LevelFiguresParamsList.TrueForAll(levelFigureParams => levelFigureParams.Completed);

        public void StartLevel(LevelParamsSnapshot levelParamsSnapshot, LevelHudHandler levelHudHandler, PackInfo packInfo)
        {
            if (levelParamsSnapshot == null)
            {
                LoggerService.LogError($"{nameof(LevelParamsSnapshot)} cannot be null");
                return;
            }
            
            _packInfo = packInfo;
            _currentLevelParams =  levelParamsSnapshot;
            var levelId = $"{_progressController.CurrentPackId}-{_progressController.CurrentLevelId}";
            _levelInfoTrackerService.StartLevelTracking(levelId);
            
            SetupHud(levelParamsSnapshot, levelHudHandler);
            
            _soundHandler.PlaySound("start");
        }

        public IPromise HideHUD(bool fast = false)
        {
            if (fast || _levelHudHandler == null)
                return Promise.Resolved();

            return _levelHudHandler.HideScreen();
        }

        private void OnDestroy()
        {
            TryTerminateCoroutine();
        }

        private void TryHandleLevelCompletion()
        {
            if (!_levelHelperService.IsLevelCompeted(_currentLevelParams))
            {
                return;
            }

            var levelPlayedTime = _levelInfoTrackerService.CurrentLevelPlayingTime;
            ResetLevelParams();

            var packId = _progressController.CurrentPackId;
            var levelId = _currentLevelParams.LevelId;
            
            var levelStatus = _progressProvider.IsLevelCompleted(packId, levelId)
                    ? CompletedLevelStatus.Replayed
                    : CompletedLevelStatus.InitialCompletion;
            
            var maybeOldEarnedStars = _progressProvider.GetEarnedStarsForLevel(packId, levelId);
            var initialStarsForLevel = maybeOldEarnedStars ?? 0;
            var earnedStarsForLevel = _levelHelperService.EvaluateEarnedStars(_currentLevelParams, levelPlayedTime);
            
            _levelHudHandler.SetInteractivity(false);
            _levelHudHandler.PlayFinishParticles();
            
            StartLevelCompletionSequence(packId, levelId, earnedStarsForLevel, initialStarsForLevel, levelPlayedTime, levelStatus);
        }

        private void StartLevelCompletionSequence(int packId, int levelId, int currentStarsForLevel, int initialStarsForLevel, float levelPlayedTime, CompletedLevelStatus completedLevelStatus)
        {
            StateMachine
                .CreateMachine(new FinishLevelContext(packId, levelId, currentStarsForLevel, initialStarsForLevel, levelPlayedTime, _packInfo, completedLevelStatus))
                .StartSequence<ShowCompletePopupState>()
                .FinishWith(this);
        }

        private void SetupHud(LevelParamsSnapshot packParam, LevelHudHandler levelHudHandler)
        {
            _levelHudHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelHudHandler>(levelHudHandler, _gameMainCanvasTransform);
            _levelHudHandler.SetupHUDFigures(packParam.LevelFiguresParamsList);
            _levelHudHandler.Initialize(_packInfo, packParam.LevelBeatingTimeInfo, packParam.FigureScale);
            
            _levelHudHandler.GetOnBeginDragFiguresSignal().ForEach(signal => signal.MapListener(StartElementDragging).DisposeWith(this));
            _levelHudHandler.GetOnDragEndFiguresSignal().ForEach(signal => signal.MapListener(EndElementDragging).DisposeWith(this));
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
            if (_levelHudHandler == null || _draggingFigureContainer == null)
                return;

            try
            {
                var releasedOnFigures = _clickHandlerService.DetectFigureTarget(eventData, _levelHudHandler.FiguresAssemblyCanvasRaycaster);
                if (releasedOnFigures.IsCollectionNullOrEmpty())
                {
                    ResetDraggingFigure();
                    return;
                }
            
                var figure = releasedOnFigures.FirstOrDefault(i => i != null && i.FigureId == _draggingFigureContainer.FigureId);
                if (figure == null)
                {
                    ResetDraggingFigure();
                    return;
                }
                
                if (_draggingFigureContainer.FigureId == figure.FigureId)
                {
                    _uiBlockHandler.BlockUserInput(true);
                    _soundHandler.PlaySound("success");
                    var shiftingAnimationPromise = new Promise();
                    _levelHudHandler.TryShiftAllElementsAfterRemoving(_draggingFigureContainer.FigureId, shiftingAnimationPromise);
                
                    TrySetFigureInserted(figure.FigureId);
                
                    _completeDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigureImage.transform.DOScale(0, 0.3f))
                        .KillWith(this);

                    Promise.All(shiftingAnimationPromise, _completeDraggingAnimationSequence.AsPromise(), figure.SetConnected())
                        .Then(SetMenuFigureConnected)
                        .Then(() => _coroutineService.WaitFor(0.05f))
                        .Then(() =>
                        {
                            SetDraggingFinished();
                            _uiBlockHandler.BlockUserInput(false);
                            TryHandleLevelCompletion();
                        })
                        .Catch(e =>
                        {
                            LoggerService.LogWarning(this, e.Message);
                            _uiBlockHandler.BlockUserInput(false);
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
                _uiBlockHandler.BlockUserInput(false);
            }
        }

        private IPromise SetMenuFigureConnected()
        {
            var menuFigurePromise = new Promise();
            _menuFigure.SetConnected(menuFigurePromise);
            
            return menuFigurePromise.Then(() =>
            {
                _levelHudHandler.DestroyFigure(_draggingFigureContainer.FigureId);
                ClearDraggingFigure();
                return _coroutineService.WaitFrame();
            }).CancelWith(this);
        }

        private void ResetDraggingFigure()
        {
            _uiBlockHandler.BlockUserInput(true);
            _soundHandler.PlaySound("fail");
            var shiftingAnimationPromise = new Promise();
            _levelHudHandler.ShiftAllElements(true, _draggingFigureContainer.FigureId, shiftingAnimationPromise);

            _resetDraggingAnimationSequence = DOTween.Sequence()
                .Append(_draggingFigureImage.transform.DOMove(_draggingFigureContainer.InitialPosition, 0.4f))
                .Join(_draggingFigureContainer.transform.DOScale(1, 0.3f))
                .Join(_draggingFigureContainer.ContainerTransform.DOSizeDelta
                    (new Vector2(_draggingFigureContainer.InitialWidth, _draggingFigureContainer.InitialHeight), 0.3f))
                .KillWith(this);

            Promise.All(shiftingAnimationPromise, _resetDraggingAnimationSequence.AsPromise())
                .Then(() => _levelHudHandler.ReturnFigureBackToScroll(_draggingFigureContainer.FigureId))
                .Then(() =>
                {
                    _draggingFigureContainer.FigureTransform.transform.localPosition = Vector3.zero;
                    ClearDraggingFigure();
                    return Promise.Resolved();
                })
                .Then(() => _coroutineService.WaitFor(0.15f))
                .Then(() =>
                {
                    SetDraggingFinished();
                    _uiBlockHandler.BlockUserInput(false);
                })
                .Catch(e =>
                {
                    LoggerService.LogWarning(this, e.Message);
                    _uiBlockHandler.BlockUserInput(false);
                })
                .CancelWith(this);
        }

        private void ClearDraggingFigure()
        {
            _levelHudHandler.LockScroll(false);
            _draggingFigureContainer = null;
            _draggingFigureImage = null;
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
            if (_draggingFigureContainer == null || _draggingFigureImage == null || !_isDraggable)
            {
                return;
            }

            _draggingFigureImage.transform.position = Input.mousePosition;
        }

        private void GoToLevelsMenu()
        {
            _screenHandler.ShowChooseLevelScreen(_packInfo);
        }

        //TODO ADD RESTART LEVEL FUNCTIONALITY
        private void RestartLevel()
        {
            // _levelHudHandler.ResetHandler();
            //
            // TryTerminateCoroutine();
            // ResetAnimationSequences();
            //
            // if (_clickHandler != null)
            // {
            //     _clickHandler.enabled = false;
            // }
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
                LoggerService.LogWarning($"Could not update progress with figure id {figureId} in {this}");
                return;
            }
            
            levelFigure.SetLevelCompleted(true);
        }
        
        private void TryTerminateCoroutine()
        {
            if (_finishCoroutine != null)
                StopCoroutine(_finishCoroutine);
        }

        public void Dispose()
        {
            if (_levelHudHandler != null)
            {
                _levelHudHandler.Dispose();
                Destroy(_levelHudHandler.gameObject);
            }
        }
        
        private void ResetLevelParams()
        {
            _levelInfoTrackerService.StopLevelTracking();
            _levelInfoTrackerService.ClearData();
        }
    }
}
