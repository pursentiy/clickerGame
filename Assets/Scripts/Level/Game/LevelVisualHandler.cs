using System;
using System.Collections.Generic;
using Figures.Animals;
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
        [Inject] private FiguresStorage _figuresStorage;
        [Inject] private ObjectsPoolHandler _objectsPoolHandler;

        [SerializeField] private Transform _figuresParentTransform;
        [SerializeField] private Camera _textureCamera;
        
        private List<FigureAnimalTarget> _figureAnimalsTargetList;
        
        public Camera TextureCamera => _textureCamera;

        protected override void Awake()
        {
            base.Awake();
            
            _figureAnimalsTargetList = new List<FigureAnimalTarget>();
        }

        public void SetupLevel(List<LevelFigureParams> levelFiguresParams, Color defaultColor)
        {
            levelFiguresParams.ForEach(figure => SetFigure(figure, defaultColor));
        }

        private void SetFigure(LevelFigureParams figureParams, Color defaultColor)
        {
            var figureObj = _objectsPoolHandler.GetPoolPrefab(PoolType.Game);

            if (figureObj == null)
            {
                Debug.LogWarning($"Could not find figure with type {figureParams.FigureType} in {this}");
                return;
            }

            figureObj.AddComponent(typeof(SpriteRenderer));
            var figure = figureObj.AddComponent<FigureAnimalTarget>();
            
            figure.transform.SetParent(_figuresParentTransform);
            figure.SetUpFigure(_figuresStorage.GetSpriteByType(figureParams.FigureType), figureParams.Completed ? figureParams.Color : defaultColor, figureParams.Scale, figureParams.Position);
            figure.SetUpDefaultParamsFigure(figureParams.Color, figureParams.FigureType);
            figure.GetPoolObjectComponent();
            _figureAnimalsTargetList.Add(figure);
        }

        private void OnDestroy()
        {
            ResetPoolObjects();
        }

        public void ResetPoolObjects()
        {
            _figureAnimalsTargetList.ForEach(figure => { figure.PoolObject.ResetObject(); });
        }
    }
}
