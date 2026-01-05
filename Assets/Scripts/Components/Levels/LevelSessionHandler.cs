using System.Collections;
using System.Linq;
using Components.Levels.Figures;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Installers;
using Level.Game;
using Level.Hud;
using Level.Widgets;
using Popup.CompleteLevelInfoPopup;
using RSG;
using Services;
using Storage;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Disposable;
using Zenject;

namespace Components.Levels
{
    public class LevelSessionHandler : InjectableMonoBehaviour, IDisposableHandlers
    {
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private PlayerService _playerService;
        [Inject] private LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private LevelHelperService _levelHelperService;
        [Inject] private UIManager _uiManager;
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private ClickHandlerService _clickHandlerService;

        [SerializeField] private RectTransform _gameMainCanvasTransform;
        [SerializeField] private RectTransform _draggingTransform;
        [SerializeField] private ParticleSystem _finishedFigureParticles;

        private LevelHudHandler _levelHudHandler;
        private FigureMenu _draggingFigureContainer;
        private GameObject _draggingFigureImage;
        private FigureMenu _menuFigure;
        private bool _isDraggable;
        private LevelParamsSnapshot _currentLevelParams;

        private Sequence _resetDraggingAnimationSequence;
        private Sequence _completeDraggingAnimationSequence;
        private Coroutine _finishCoroutine;
        
        private bool IsLevelComplete => _currentLevelParams != null && 
                                        _currentLevelParams.LevelFiguresParamsList.TrueForAll(levelFigureParams => levelFigureParams.Completed);

        public void StartLevel(LevelParamsSnapshot levelParamsSnapshot, LevelHudHandler levelHudHandler, Color defaultColor)
        {
            if (levelParamsSnapshot == null)
            {
                LoggerService.LogError($"{nameof(LevelParamsSnapshot)} cannot be null");
                return;
            }
            _currentLevelParams =  levelParamsSnapshot;
            var levelId = $"{_playerProgressService.CurrentPackNumber}-{_playerProgressService.CurrentLevelNumber}";
            _levelInfoTrackerService.StartLevelTracking(levelId);
            
            SetupHud(levelParamsSnapshot, levelHudHandler);
            
            _soundHandler.PlaySound("start");
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
            _levelInfoTrackerService.StopLevelTracking();
            _levelInfoTrackerService.ClearData();

            var maybeOldEarnedStars = _playerProgressService.GetEarnedStarsForLevel(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber);
            var earnedStars = _levelHelperService.EvaluateEarnedStars(_currentLevelParams, levelPlayedTime);
            var starsForAccrual = _levelHelperService.EvaluateStarsForAccrual(earnedStars, maybeOldEarnedStars);
            _playerService.SetLevelCompleted(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber, levelPlayedTime, earnedStars);
            
            _levelHudHandler.SetInteractivity(false);
            _playerProgressService.TrySetOrUpdateLevelCompletion(_playerProgressService.CurrentPackNumber, _playerProgressService.CurrentLevelNumber, earnedStars, levelPlayedTime);
            _finishCoroutine = StartCoroutine(AwaitFinishLevel(earnedStars, starsForAccrual, levelPlayedTime));
        }

        private IEnumerator AwaitFinishLevel(int totalStars, int starsForAccrual, float levelPlayedTime)
        {
            yield return new WaitForSeconds(_screenHandler.AwaitChangeScreenTime);
            _soundHandler.PlaySound("finished");
            var context = new CompleteLevelInfoPopupContext(totalStars, starsForAccrual, levelPlayedTime, GoToLevelsMenu);
            _uiManager.PopupsHandler.ShowPopupImmediately<CompleteLevelInfoPopupMediator>(context);
        }

        private void SetupHud(LevelParamsSnapshot packParam, LevelHudHandler levelHudHandler)
        {
            _levelHudHandler = ContainerHolder.CurrentContainer.InstantiatePrefabForComponent<LevelHudHandler>(levelHudHandler, _gameMainCanvasTransform);
            _levelHudHandler.SetupHUDFigures(packParam.LevelFiguresParamsList);
            _levelHudHandler.Initialize(packParam.LevelBeatingTimeInfo);
            
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
            
            var releasedOnFigure = _clickHandlerService.DetectFigureTarget(eventData, _levelHudHandler.FiguresAssemblyCanvasRaycaster);
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
                            TryHandleLevelCompletion();
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

        private void GoToLevelsMenu()
        {
            _screenHandler.ShowChooseLevelScreen();
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
    }
}
