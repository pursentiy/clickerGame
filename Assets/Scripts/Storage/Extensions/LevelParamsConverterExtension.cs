using System.Linq;
using Storage.Levels;
using Storage.Snapshots.LevelParams;

namespace Storage.Extensions
{
    public static class LevelParamsConverterExtension
    {
        public static LevelParamsSnapshot ToSnapshot(this LevelParamsData levelParams)
        {
            if (levelParams.LevelBeatingTimeInfo == null) 
                return null;

            var figuresParams = levelParams.LevelsFiguresParams
                .Select(i => i.ToSnapshot())
                .Where(i => i != null)
                .ToList();
            
            return new LevelParamsSnapshot(levelParams.LevelId, levelParams.FigureScale,  levelParams.LevelBeatingTimeInfo.ToSnapshot(), figuresParams);
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

        private static LevelFigureParamsSnapshot ToSnapshot(this LevelFigureParamsData levelFigureParams)
        {
            if (levelFigureParams == null)
                return null;
            
            return new LevelFigureParamsSnapshot(levelFigureParams.FigureId, false);
        }
    }
}