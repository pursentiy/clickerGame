using System.Collections.Generic;
using System.Linq;
using Components.Levels.Figures;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Installers;
using Level.Widgets;
using Plugins.FSignal;
using Popup.Settings;
using RSG;
using Services;
using Storage;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Level.Hud
{
    public class LevelHudHandler : InjectableMonoBehaviour, IDisposableHandlers
    {
        [Inject] private LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PopupHandler _popupHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private UIManager _uiManager;

        [SerializeField] private RectTransform _figuresDraggingContainer;
        [SerializeField] private RectTransform _figuresAssemblyContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private HorizontalLayoutGroup _figuresGroup;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private StarsProgressWidget _starsProgressWidget;
        [SerializeField] private LevelTimerWidget _levelTimerWidget;
        [SerializeField] private GraphicRaycaster _figuresAssemblyCanvasRaycaster;
        [SerializeField] private ParticleSystem _finishLevelParticles;
        
        private List<FigureMenu> _figureAnimalsForAssemblyList = new List<FigureMenu>();
        private List<FigureTarget> _figureAnimalsTargetList = new List<FigureTarget>();
        private float _figuresGroupSpacing;
        private Sequence _shiftingSequence;
        
        public FSignal BackToMenuClickSignal { get; } = new FSignal();
        public GraphicRaycaster FiguresAssemblyCanvasRaycaster => _figuresAssemblyCanvasRaycaster;

        public void ResetHandler()
        {
            _starsProgressWidget.ResetWidget();
            OnTimerChanged(_levelInfoTrackerService.CurrentLevelPlayingTime);
        }

        public void Initialize(LevelBeatingTimeInfoSnapshot levelBeatingTime, float assemblyContainerScale)
        {
            _levelInfoTrackerService.CurrentLevelPlayingTimeChangedSignal.MapListener(OnTimerChanged).DisposeWith(this);
            _starsProgressWidget.Initialize(levelBeatingTime);
            SetAssemblyContainerScale(assemblyContainerScale);
            OnTimerChanged(_levelInfoTrackerService.CurrentLevelPlayingTime);
        }

        public void SetupHUDFigures(List<LevelFigureParamsSnapshot> levelFiguresParams)
        {
            levelFiguresParams.ForEach(SetDraggingFigure);
            levelFiguresParams.ForEach(SetAssemblyContainerFigure);
        }

        private void SetAssemblyContainerScale(float scale)
        {
            if (scale <= 0)
                return;
            
            _figuresAssemblyContainer.localScale = new Vector3(scale, scale, scale);
        }
        
        protected override void Awake()
        {
            _backButton.onClick.MapListenerWithSound(GoToMainMenuScreen).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null)).DisposeWith(this);

            _figuresGroupSpacing = _figuresGroup.spacing;
        }
        
        public void SetInteractivity(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
        }
        
        private void OnTimerChanged(float seconds)
        {
            _starsProgressWidget.OnTimeUpdate(seconds);
            _levelTimerWidget.UpdateTime(seconds);
        }

        private void SetDraggingFigure(LevelFigureParamsSnapshot figureParams)
        {
            var figurePrefab = _levelsParamsStorageData.GetMenuFigure(_playerProgressService.CurrentPackNumber,
                _playerProgressService.CurrentLevelNumber, figureParams.FigureId);

            if (figurePrefab == null)
            {
                LoggerService.LogWarning($"Could not find figure with type {figureParams.FigureId} in {this}");
                return;
            }

            if (figureParams.Completed)
            {
                return;
            }
            
            var figure = Instantiate(figurePrefab, _figuresDraggingContainer);
            figure.SetUpDefaultParamsFigure(figureParams.FigureId);
            figure.SetScale(1);
            _figureAnimalsForAssemblyList.Add(figure);
            
            SetupDraggingSignalsHandlers(figure);
        }
        
        private void SetAssemblyContainerFigure(LevelFigureParamsSnapshot figureParams)
        {
            var figurePrefab = _levelsParamsStorageData.GetTargetFigure(_playerProgressService.CurrentPackNumber,
                _playerProgressService.CurrentLevelNumber, figureParams.FigureId);

            if (figurePrefab == null)
            {
                LoggerService.LogWarning($"Could not find figure with type {figureParams.FigureId} in {this}");
                return;
            }

            var figure = Instantiate(figurePrefab, _figuresAssemblyContainer);
            figure.SetUpFigure(figureParams.Completed);
            figure.SetUpDefaultParamsFigure(figureParams.FigureId);
            _figureAnimalsTargetList.Add(figure);
        }

        private void SetupDraggingSignalsHandlers(FigureMenu figure)
        {
            figure.OnBeginDragSignal.MapListener(OnBeginDragSignalHandler).DisposeWith(this);
            figure.OnDraggingSignal.MapListener(OnDraggingSignalHandler).DisposeWith(this);
            figure.OnEndDragSignal.MapListener(OnEndDragSignalHandler).DisposeWith(this);
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
            if (_figureAnimalsForAssemblyList.Count == 0)
            {
                animationPromise.Resolve();
                return;
            }
            
            if(_shiftingSequence != null && _shiftingSequence.IsActive())
                _shiftingSequence.Complete();

            _shiftingSequence = DOTween.Sequence();
            _figuresGroup.enabled = false;

            _figureAnimalsForAssemblyList.ForEach(figure =>
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
            if (_figureAnimalsForAssemblyList.Count <= 1)
            {
                animationPromise.Resolve();
                return;
            }
            
            var shiftingSequence = DOTween.Sequence();
            _figureAnimalsForAssemblyList.ForEach(figure =>
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
            _figureAnimalsForAssemblyList.FirstOrDefault(figure => figure.FigureId == figureId)?.Destroy();
            _figureAnimalsForAssemblyList = _figureAnimalsForAssemblyList.Where(figure => figure.FigureId != figureId).ToList();
        }

        public void LockScroll(bool value)
        {
            _scrollRect.horizontal = !value;
        }
        
        public void PlayFinishParticles()
        {
            if (_finishLevelParticles.isPlaying)
            {
                _finishLevelParticles.Stop();
            }

            _finishLevelParticles.Simulate(0);
            _finishLevelParticles.Play();
        }

        public FigureMenu GetFigureById(int figureId)
        {
            return _figureAnimalsForAssemblyList.FirstOrDefault(figure => figure.FigureId == figureId);
        }

        public List<FSignal<FigureMenu>> GetOnBeginDragFiguresSignal()
        {
            return _figureAnimalsForAssemblyList.Select(figure => figure.OnBeginDragFigureSignal).ToList();
        }

        public List<FSignal<PointerEventData>> GetOnDragEndFiguresSignal()
        {
            return _figureAnimalsForAssemblyList.Select(figure => figure.OnEndDragSignal).ToList();
        }

        public void ReturnFigureBackToScroll(int figureId)
        {
            var figure = GetFigureById(figureId);
            figure.FigureTransform.SetParent(figure.ContainerTransform);
        }

        public void Dispose()
        {
            _levelInfoTrackerService.CurrentLevelPlayingTimeChangedSignal.RemoveListener(OnTimerChanged);
        }
    }
}