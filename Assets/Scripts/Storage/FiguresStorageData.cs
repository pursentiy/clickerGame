using System.Collections.Generic;
using System.Linq;
using Figures;
using Figures.Animals;
using Level.Game;
using Storage.Levels;
using UnityEngine;

namespace Storage
{
    [CreateAssetMenu(fileName = "FiguresStorage", menuName = "ScriptableObjects/FiguresStorage")]
    public class FiguresStorageData : ScriptableObject
    {
        [SerializeField] private List<PackParamsData> _packParamsList;

        public FigureTarget GetTargetFigure(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packNumber, levelNumber, figureId);

            return levelParams?.FigureTarget;
        }
        
        public FigureMenu GetMenuFigure(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packNumber, levelNumber, figureId);

            return levelParams?.FigureMenu;
        }
        
        public LevelFigureParamsData GetLevelFiguresParamsData(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelParamsData(packNumber, levelNumber);

            return levelParams?.LevelsFiguresParams.FirstOrDefault(levelFigureParams => levelFigureParams.FigureId == figureId);
        }

        public LevelVisualHandler GetLevelVisualHandler(int packNumber, int levelNumber)
        {
            var levelParams = GetLevelParamsData(packNumber, levelNumber);

            return levelParams?.LevelVisualHandler;
        }

        public LevelParamsData GetLevelParamsData(int packNumber, int levelNumber)
        {
            return _packParamsList.FirstOrDefault(figuresParams => figuresParams.PackNumber == packNumber)?
                .LevelsParams.FirstOrDefault(levelParams => levelParams.LevelNumber == levelNumber);
        }
    }
}