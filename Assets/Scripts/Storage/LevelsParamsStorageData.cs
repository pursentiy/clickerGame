using System.Collections.Generic;
using System.Linq;
using Components.Levels.Figures;
using Storage.Levels;
using UnityEngine;

namespace Storage
{
    [CreateAssetMenu(fileName = "FiguresStorage", menuName = "ScriptableObjects/FiguresStorage")]
    public class LevelsParamsStorageData : ScriptableObject
    {
        [SerializeField] private List<PackParamsData> _packParamsList;
        
        public List<PackParamsData> DefaultPacksParamsList => _packParamsList;

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

        public LevelParamsData GetLevelParamsData(int packNumber, int levelNumber)
        {
            return GetPackParamsData(packNumber)?.LevelsParams.FirstOrDefault(levelParams => levelParams.LevelNumber == levelNumber);
        }

        public PackParamsData GetPackParamsData(int packNumber)
        {
            return _packParamsList.FirstOrDefault(figuresParams => figuresParams.PackNumber == packNumber);
        }
    }
}