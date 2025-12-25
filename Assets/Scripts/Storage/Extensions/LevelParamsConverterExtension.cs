using System.Linq;
using Storage.Levels.Params;
using Storage.Snapshots.LevelParams;

namespace Storage.Extensions
{
    public static class LevelParamsConverterExtension
    {
        public static LevelParamsSnapshot ToSnapshot(this LevelParams levelParams)
        {
            if (levelParams.LevelBeatingTimeInfo == null) 
                return null;

            var figuresParams = levelParams.LevelFiguresParamsList
                .Select(i => i.ToSnapshot())
                .Where(i => i != null)
                .ToList();
            
            return new LevelParamsSnapshot(levelParams.LevelNumber,  levelParams.LevelBeatingTimeInfo.ToSnapshot(), figuresParams);
        }

        private static LevelBeatingTimeInfoSnapshot ToSnapshot(this LevelBeatingTimeInfo levelBeatingTimeInfo)
        {
            if (levelBeatingTimeInfo == null)
                return null;
            
            return new LevelBeatingTimeInfoSnapshot(
                levelBeatingTimeInfo.FastestTime,
                levelBeatingTimeInfo.MediumTime,
                levelBeatingTimeInfo.MinimumTime);
        }

        private static LevelFigureParamsSnapshot ToSnapshot(this LevelFigureParams levelFigureParams)
        {
            if (levelFigureParams == null)
                return null;
            
            return new LevelFigureParamsSnapshot(levelFigureParams.FigureId, false);
        }
    }
}