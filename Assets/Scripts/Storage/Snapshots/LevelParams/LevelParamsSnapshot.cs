using System.Collections.Generic;

namespace Storage.Snapshots.LevelParams
{
    public class LevelParamsSnapshot
    {
        public LevelParamsSnapshot(int levelId, float figureScale, LevelBeatingTimeInfoSnapshot levelBeatingTimeInfo, List<LevelFigureParamsSnapshot> levelFiguresParamsList)
        {
            LevelId = levelId;
            LevelBeatingTimeInfo = levelBeatingTimeInfo;
            LevelFiguresParamsList = levelFiguresParamsList;
            FigureScale = figureScale;
        }

        public int LevelId { get; }
        public float FigureScale { get; }
        public LevelBeatingTimeInfoSnapshot LevelBeatingTimeInfo { get; }
        public List<LevelFigureParamsSnapshot> LevelFiguresParamsList { get; }
    }
}