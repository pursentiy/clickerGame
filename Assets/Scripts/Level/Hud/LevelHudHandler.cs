using System;
using System.Collections.Generic;
using System.Linq;
using Figures;
using Figures.Animals;
using Handlers;
using Installers;
using Plugins.FSignal;
using Pooling;
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
        [Inject] private FiguresStorage _figuresStorage;
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ObjectsPoolHandler _objectsPoolHandler;

        [SerializeField] private RectTransform _figuresParentTransform;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        
        private List<FigureAnimalsMenu> _figureAnimalsMenuList;

        protected override void Awake()
        {
            _figureAnimalsMenuList = new List<FigureAnimalsMenu>();
            
            _backButton.onClick.AddListener(GoToMainMenuScreen);
        }

        public FSignal BackToMenuClickSignal { get; } = new FSignal();

        public void SetupScrollMenu(List<LevelFigureParams> levelFiguresParams)
        {
            levelFiguresParams.ForEach(SetFigure);
        }

        private void SetFigure(LevelFigureParams figureParams)
        {
            var figureObj = _objectsPoolHandler.GetPoolPrefab(PoolType.Canvas);

            if (figureObj == null)
            {
                Debug.LogWarning($"Could not find figure with type {figureParams.FigureType} in {this}");
                return;
            }

            if (figureParams.Completed)
            {
                return;
            }

            figureObj.AddComponent<Image>();
            var figure = figureObj.AddComponent<FigureAnimalsMenu>();

            figure.transform.SetParent(_figuresParentTransform);
            figure.SetUpDefaultParamsFigure(figureParams.Color, figureParams.FigureType);
            figure.SetUpFigure(_figuresStorage.GetSpriteByType(figureParams.FigureType), figure.FigureColor);
            figure.SetScale(1);
            figure.GetPoolObjectComponent();
            _figureAnimalsMenuList.Add(figure);
            
            SetupDraggingSignalsHandlers(figure);
        }

        private void SetupDraggingSignalsHandlers(FigureAnimalsMenu figure)
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

        public FigureAnimalsMenu GetFigureByType(FigureType type)
        {
            return _figureAnimalsMenuList.FirstOrDefault(figure => figure.FigureType == type);
        }

        public List<FSignal<FigureAnimalsMenu>> GetOnBeginDragFiguresSignal()
        {
            return _figureAnimalsMenuList.Select(figure => figure.OnBeginDragFigureSignal).ToList();
        }

        public List<FSignal<PointerEventData>> GetOnDragEndFiguresSignal()
        {
            return _figureAnimalsMenuList.Select(figure => figure.OnEndDragSignal).ToList();
        }

        public void ReturnFigureBackToScroll(FigureType type)
        {
            var figure = GetFigureByType(type);
            figure.transform.SetParent(_figuresParentTransform);
            figure.transform.SetSiblingIndex(figure.SiblingPosition);
        }
        
        public void ResetPoolObjects()
        {
            _figureAnimalsMenuList.ForEach(figure => { figure.PoolObject.ResetObject(); });
        }

        private void OnDestroy()
        {
            ResetPoolObjects();
            
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