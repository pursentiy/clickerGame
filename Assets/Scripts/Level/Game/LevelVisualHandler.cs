using System;
using System.Collections.Generic;
using Figures.Animals;
using Handlers;
using Installers;
using Pooling;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Level.Game
{
    public class LevelVisualHandler : InjectableMonoBehaviour, ILevelVisualHandler
    {
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private ProgressHandler _progressHandler;

        [SerializeField] private Transform _figuresParentTransform;
        [SerializeField] private Camera _textureCamera;
        [SerializeField] private CanvasGroup _canvasGroup;

        private List<FigureTarget> _figureAnimalsTargetList;
        
        public Camera TextureCamera => _textureCamera;

        protected override void Awake()
        {
            base.Awake();
            
            _figureAnimalsTargetList = new List<FigureTarget>();
        }

        public void SetInteractivity(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
        }

        public void SetupLevel(List<LevelFigureParams> levelFiguresParams, Color defaultColor)
        {
            levelFiguresParams.ForEach(figure => SetFigure(figure, defaultColor));
        }

        private void SetFigure(LevelFigureParams figureParams, Color defaultColor)
        {
            var figurePrefab = _figuresStorageData.GetTargetFigure(_progressHandler.CurrentPackNumber,
                _progressHandler.CurrentLevelNumber, figureParams.FigureId);

            if (figurePrefab == null)
            {
                Debug.LogWarning($"Could not find figure with type {figureParams.FigureId} in {this}");
                return;
            }

            var figure = Instantiate(figurePrefab, _figuresParentTransform);
            figure.SetUpFigure(figureParams.Completed);
            figure.SetUpDefaultParamsFigure(figureParams.FigureId);
            _figureAnimalsTargetList.Add(figure);
        }

        private void OnDestroy()
        {
            _figureAnimalsTargetList.ForEach(figure =>
            {
                Destroy(figure.gameObject);
            });
        }
    }
}
