using System.Collections.Generic;
using System.Linq;
using Common.Data.Info;
using Common.Widgets.Animations;
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
using Services.CoroutineServices;
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
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly CoroutineService _coroutineService;

        [SerializeField] private RectTransform _figuresDraggingContainer;
        [SerializeField] private RectTransform _figuresAssemblyContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private HorizontalLayoutGroup _figuresLayoutGroup;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private StarsProgressWidget _starsProgressWidget;
        [SerializeField] private LevelTimerWidget _levelTimerWidget;
        [SerializeField] private GraphicRaycaster _figuresAssemblyCanvasRaycaster;
        [SerializeField] private ParticleSystem _finishLevelParticles;
        [SerializeField] private FadeWidget _fadeWidget;
        
        private List<FigureMenu> _figureAnimalsForAssemblyList = new List<FigureMenu>();
        private List<FigureTarget> _figureAnimalsTargetList = new List<FigureTarget>();
        private float _figuresGroupSpacing;
        private Sequence _shiftingSequence;
        private PackInfo _currentPackInfo;
        
        public FSignal BackToMenuClickSignal { get; } = new FSignal();
        public GraphicRaycaster FiguresAssemblyCanvasRaycaster => _figuresAssemblyCanvasRaycaster;

        public void ResetHandler()
        {
            _starsProgressWidget.ResetWidget();
            OnTimerChanged(_levelInfoTrackerService.CurrentLevelPlayingTime);
        }
        
        public IPromise HideScreen()
        {
            return _fadeWidget.Hide();
        }

        public void SetInteractivity(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
        }

        public void Initialize(PackInfo currentPackInfo, LevelBeatingTimeInfoSnapshot levelBeatingTime, float assemblyContainerScale)
        {
            LoggerService.LogDebugEditor($"{nameof(LevelBeatingTimeInfoSnapshot)}: \n" +
                                         $"{nameof(levelBeatingTime.FastestTime)} - {levelBeatingTime.FastestTime}\n" +
                                         $"{nameof(levelBeatingTime.MediumTime)} - {levelBeatingTime.MediumTime}\n" +
                                         $"{nameof(levelBeatingTime.MinimumTime)} - {levelBeatingTime.MinimumTime}");
            
            _levelInfoTrackerService.CurrentLevelPlayingTimeChangedSignal.MapListener(OnTimerChanged).DisposeWith(this);
            _starsProgressWidget.Initialize(levelBeatingTime);
            SetAssemblyContainerScale(assemblyContainerScale);
            OnTimerChanged(_levelInfoTrackerService.CurrentLevelPlayingTime);
            _currentPackInfo = currentPackInfo;
        }

        public void SetupHUDFigures(List<LevelFigureParamsSnapshot> levelFiguresParams)
        {
            levelFiguresParams.ForEach(SetDraggingFigure);
            levelFiguresParams.ForEach(SetAssemblyContainerFigure);
            
            _coroutineService.WaitFrame().Then(() => LayoutRebuilder.ForceRebuildLayoutImmediate(_figuresDraggingContainer))
                .CancelWith(this);
        }
        
        private void Start()
        {
            _fadeWidget.Show();
        }
        
        protected override void Awake()
        {
            _fadeWidget.ResetWidget();
            _backButton.onClick.MapListenerWithSound(GoToMainMenuScreen).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(ShowSettingsPopup).DisposeWith(this);

            _figuresGroupSpacing = _figuresLayoutGroup.spacing;
        }

        private void ShowSettingsPopup()
        {
            var context = new SettingsPopupContext(false);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }
        
        private void SetAssemblyContainerScale(float scale)
        {
            if (scale <= 0)
                return;
            
            _figuresAssemblyContainer.localScale = new Vector3(scale, scale, scale);
        }
        
        private void OnTimerChanged(double seconds)
        {
            _starsProgressWidget.OnTimeUpdate(seconds);
            _levelTimerWidget.UpdateTime(seconds);
        }

        private void SetDraggingFigure(LevelFigureParamsSnapshot figureParams)
        {
            var figurePrefab = _levelsParamsStorageData.GetMenuFigure(_progressController.CurrentPackId,
                _progressController.CurrentLevelId, figureParams.FigureId);

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
            var figurePrefab = _levelsParamsStorageData.GetTargetFigure(_progressController.CurrentPackId,
                _progressController.CurrentLevelId, figureParams.FigureId);

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
            _screenHandler.ShowChooseLevelScreen(_currentPackInfo, BackToMenuClickSignal);
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