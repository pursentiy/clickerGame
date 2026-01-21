using System.Linq;
using Common.Data.Info;
using Extensions;
using Services;
using Storage.Levels;
using Storage.Snapshots.LevelParams;

namespace Storage.Extensions
{
    public static class LevelParamsConverterExtension
    {
        public static LevelParamsSnapshot ToSnapshot(this LevelInfo levelInfo)
        {
            if (levelInfo == null) 
                return null;

            var figuresParams = levelInfo.LevelsFiguresInfo
                .Select(i => i.ToSnapshot())
                .Where(i => i != null)
                .ToList();
            
            return new LevelParamsSnapshot(levelInfo.LevelId, levelInfo.FigureScale,  ToSnapshot(levelInfo.BeatingTime), figuresParams);
        }

        private static LevelBeatingTimeInfoSnapshot ToSnapshot(float[] beatingTime)
        {
            if (beatingTime.IsCollectionNullOrEmpty() || beatingTime.Length != 3)
            {
                LoggerService.LogWarning($"[{nameof(LevelBeatingTimeInfoSnapshot)}]: {nameof(beatingTime)} is empty or does not contain 3 values");
                return new LevelBeatingTimeInfoSnapshot(3, 6, 9);
            }
            
            //TODO POSSIBLE EXCEPTION
            return new LevelBeatingTimeInfoSnapshot(
                beatingTime[0],
                beatingTime[1],
                beatingTime[2]);
        }

        private static LevelFigureParamsSnapshot ToSnapshot(this FigureInfo levelFigureParams)
        {
            if (levelFigureParams == null)
                return null;
            
            return new LevelFigureParamsSnapshot(levelFigureParams.FigureId, false);
        }
    }
}