using System.Collections.Generic;
using System.Linq;
using Extensions;
using Storage.Levels;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine;

namespace Storage
{
    [CreateAssetMenu(fileName = "FiguresStorage", menuName = "ScriptableObjects/FiguresStorage")]
    public class LevelsParamsStorageData : ScriptableObject
    {
        [SerializeField] private List<PackParamsData> _packParamsList;
        
        public List<PackParamsData> DefaultPacksParamsList => _packParamsList;

        public FigureTargetWidget GetTargetFigure(int packId, int levelId, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packId, levelId, figureId);

            return levelParams?.FigureTarget;
        }
        
        public FigureMenuWidget GetMenuFigure(int packId, int levelId, int figureId)
        {
            var levelParams = GetLevelFiguresParamsData(packId, levelId, figureId);

            return levelParams?.FigureMenu;
        }
        
        public List<TargetFigureInfo> GetTargetFigures(int packId, int levelId)
        {
            var levelParams = GetLevelParamsData(packId, levelId);
            if (levelParams == null || levelParams.LevelsFiguresParams.IsCollectionNullOrEmpty())
                return new List<TargetFigureInfo>();
            
            return levelParams.LevelsFiguresParams.Select(i => new TargetFigureInfo(i.FigureId, i.FigureTarget)).ToList();
        }

        public List<MenuFigureInfo> GetMenuFigures(int packId, int levelId)
        {
            var levelParams = GetLevelParamsData(packId, levelId);
            if (levelParams == null || levelParams.LevelsFiguresParams.IsCollectionNullOrEmpty())
                return new List<MenuFigureInfo>();
            
            return levelParams.LevelsFiguresParams.Select(i => new MenuFigureInfo(i.FigureId, i.FigureMenu)).ToList();
        }
        
        public LevelFigureParamsData GetLevelFiguresParamsData(int packId, int levelId, int figureId)
        {
            var levelParams = GetLevelParamsData(packId, levelId);

            return levelParams?.LevelsFiguresParams.FirstOrDefault(levelFigureParams => levelFigureParams.FigureId == figureId);
        }

        public LevelParamsData GetLevelParamsData(int packId, int levelId)
        {
            return GetPackParamsData(packId)?.LevelsParams.FirstOrDefault(levelParams => levelParams.LevelId == levelId);
        }

        public PackParamsData GetPackParamsData(int packId)
        {
            return _packParamsList.FirstOrDefault(figuresParams => figuresParams.PackId == packId);
        }
    }

    public class TargetFigureInfo : FigureInfoBase
    {
        public TargetFigureInfo(int id, FigureTargetWidget widget) : base(id)
        {
            Widget = widget;
        }

        public FigureTargetWidget Widget {get;}
    }
    
    public class MenuFigureInfo : FigureInfoBase
    {
        public MenuFigureInfo(int id, FigureMenuWidget widget) : base(id)
        {
            Widget = widget;
        }

        public FigureMenuWidget Widget {get;}
    }

    public class FigureInfoBase
    {
        public FigureInfoBase(int id)
        {
            Id = id;
        }

        public int Id {get;}
    }
}