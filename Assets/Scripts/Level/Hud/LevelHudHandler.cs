using System.Collections.Generic;
using System.Linq;
using Figures.Animals;
using Handlers;
using Installers;
using Plugins.FSignal;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Level.Hud
{
    public class LevelHudHandler : InjectableMonoBehaviour, ILevelHudHandler
    {
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressHandler _progressHandler;

        [SerializeField] private RectTransform _figuresParentTransform;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        
        private List<FigureMenu> _figureAnimalsMenuList;

        protected override void Awake()
        {
            _figureAnimalsMenuList = new List<FigureMenu>();
            
            _backButton.onClick.AddListener(GoToMainMenuScreen);
        }

        public FSignal BackToMenuClickSignal { get; } = new FSignal();

        public void SetupScrollMenu(List<LevelFigureParams> levelFiguresParams)
        {
            levelFiguresParams.ForEach(SetFigure);
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
            BackToMenuClickSignal.Dispatch();
            _screenHandler.ShowChooseLevelScreen();
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
            figure.transform.SetParent(_figuresParentTransform);
            figure.transform.SetSiblingIndex(figure.SiblingPosition);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();

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