using System.Collections.Generic;
using System.Linq;
using Storage.Levels;
using UI.Screens.PuzzleAssemblyScreen.Figures;
using UnityEngine;

namespace Storage
{
    [CreateAssetMenu(fileName = "FiguresStorage", menuName = "ScriptableObjects/FiguresStorage")]
    public class LevelsParamsStorageData : ScriptableObject
    {
        [SerializeField] private List<PackParamsData> _packParamsList;
        
        public List<PackParamsData> DefaultPacksParamsList => _packParamsList;

        public FigureTargetWidget GetTargetFigure(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packNumber, levelNumber, figureId);

            return levelParams?._figureTargetWidget;
        }
        
        public FigureMenuWidget GetMenuFigure(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packNumber, levelNumber, figureId);

            return levelParams?.figureMenuWidget;
        }
        
        public LevelFigureParamsData GetLevelFiguresParamsData(int packNumber, int levelNumber, int figureId)
        {
            var levelParams = GetLevelParamsData(packNumber, levelNumber);

            return levelParams?.LevelsFiguresParams.FirstOrDefault(levelFigureParams => levelFigureParams.FigureId == figureId);
        }

        public LevelParamsData GetLevelParamsData(int packNumber, int levelNumber)
        {
            return GetPackParamsData(packNumber)?.LevelsParams.FirstOrDefault(levelParams => levelParams.LevelId == levelNumber);
        }

        public PackParamsData GetPackParamsData(int packNumber)
        {
            return _packParamsList.FirstOrDefault(figuresParams => figuresParams.PackId == packNumber);
        }
    }
}