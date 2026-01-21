using System.Collections;
using System.Linq;
using Common.Data.Info;
using Components.Levels.Figures;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Installers;
using Level.Hud;
using Level.Widgets;
using Popup.CompleteLevelInfoPopup;
using RSG;
using Services;
using Services.Player;
using Storage;
using Storage.Levels;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Disposable;
using Zenject;

namespace Components.Levels
{
    public class LevelSessionHandler : MonoBehaviour, IDisposableHandlers
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly LevelHelperService _levelHelperService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        [Inject] private readonly ClickHandlerService _clickHandlerService;
        [Inject] private readonly UIBlockHandler _uiBlockHandler;

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
            _levelInfoTrackerService.StopLevelTracking();
            _levelInfoTrackerService.ClearData();

            var earnedStars = _levelHelperService.EvaluateEarnedStars(_currentLevelParams, levelPlayedTime);
            _progressController.SetLevelCompleted(_progressController.CurrentPackId, _progressController.CurrentLevelId, levelPlayedTime, earnedStars);
            
            _levelHudHandler.SetInteractivity(false);
            _levelHudHandler.PlayFinishParticles();
            _finishCoroutine = StartCoroutine(AwaitFinishLevel(earnedStars, earnedStars, levelPlayedTime));
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
            
            var releasedOnFigures = _clickHandlerService.DetectFigureTarget(eventData, _levelHudHandler.FiguresAssemblyCanvasRaycaster);
            if (releasedOnFigures.IsCollectionNullOrEmpty())
            {
                ResetDraggingFigure();
                return;
            }
            
            var maybeFigure = releasedOnFigures.FirstOrDefault(i => i != null && i.FigureId == _draggingFigureContainer.FigureId);
            if (maybeFigure == null)
            {
                ResetDraggingFigure();
                return;
            }

            if (_draggingFigureContainer.FigureId == maybeFigure.FigureId)
            {
                _soundHandler.PlaySound("success");
                var shiftingAnimationPromise = new Promise();
                _levelHudHandler.TryShiftAllElementsAfterRemoving(_draggingFigureContainer.FigureId, shiftingAnimationPromise);
                
                TrySetFigureInserted(maybeFigure.FigureId);
                
                _completeDraggingAnimationSequence = DOTween.Sequence().Append(_draggingFigureImage.transform.DOScale(0, 0.4f))
                    .KillWith(this);

                shiftingAnimationPromise.Then(() =>
                {
                    _completeDraggingAnimationSequence.OnComplete(() =>
                        {
                            maybeFigure.SetConnected();
                            SetMenuFigureConnected();
                            TryHandleLevelCompletion();
                        });
                }).CancelWith(this);
            }
            else
            {
                ResetDraggingFigure();
            }
        }

        private void SetMenuFigureConnected()
        {
            var menuFigurePromise = new Promise();
            _menuFigure.SetConnected(menuFigurePromise);
            menuFigurePromise.Then(() =>
            {
                _levelHudHandler.DestroyFigure(_draggingFigureContainer.FigureId);
                ClearDraggingFigure();
            }).CancelWith(this);
        }

        private void ResetDraggingFigure()
        {
            _uiBlockHandler.BlockUserInput(true);
            _soundHandler.PlaySound("fail");
            var shiftingAnimationPromise = new Promise();
            _levelHudHandler.ShiftAllElements(true, _draggingFigureContainer.FigureId, shiftingAnimationPromise);

            _resetDraggingAnimationSequence = DOTween.Sequence()
                .Append(_draggingFigureImage.transform.DOMove(_draggingFigureContainer.transform.position, 0.4f))
                .Join(_draggingFigureContainer.transform.DOScale(1, 0.3f))
                .Join(_draggingFigureContainer.ContainerTransform.DOSizeDelta
                    (new Vector2(_draggingFigureContainer.InitialWidth, _draggingFigureContainer.InitialHeight), 0.3f))
                .KillWith(this);
            
            shiftingAnimationPromise.Then(() =>
            {
                _resetDraggingAnimationSequence.OnComplete(() =>
                    {
                        _levelHudHandler.ReturnFigureBackToScroll(_draggingFigureContainer.FigureId);
                        _draggingFigureContainer.FigureTransform.transform.localPosition = Vector3.zero;
                        ClearDraggingFigure();
                        _uiBlockHandler.BlockUserInput(false);
                    }).KillWith(this);
            }).CancelWith(this);
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
    }
}
