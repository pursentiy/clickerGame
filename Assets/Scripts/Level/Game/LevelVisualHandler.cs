using System.Collections.Generic;
using Animations;
using Components.Levels.Figures;
using Handlers;
using Installers;
using Services;
using Storage;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using Zenject;

namespace Level.Game
{
    public class LevelVisualHandler : InjectableMonoBehaviour, IDisposableHandlers
    {
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private PlayerProgressService _playerProgressService;

        [SerializeField] private Transform _figuresParentTransform;
        [SerializeField] private ScreenColorAnimation screenColorAnimation;

        private List<FigureTarget> _figureAnimalsTargetList;

        public ScreenColorAnimation ScreenColorAnimation => screenColorAnimation;

        protected override void Awake()
        {
            base.Awake();
            
            _figureAnimalsTargetList = new List<FigureTarget>();
        }

        public void SetupLevel(List<LevelFigureParamsSnapshot> levelFiguresParams, Color defaultColor)
        {
            levelFiguresParams.ForEach(figure => SetFigure(figure, defaultColor));
        }

        private void SetFigure(LevelFigureParamsSnapshot figureParams, Color defaultColor)
        {
            var figurePrefab = _figuresStorageData.GetTargetFigure(_playerProgressService.CurrentPackNumber,
                _playerProgressService.CurrentLevelNumber, figureParams.FigureId);

            if (figurePrefab == null)
            {
                LoggerService.LogWarning($"Could not find figure with type {figureParams.FigureId} in {this}");
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

        public void Dispose()
        {
            
        }
    }
}
