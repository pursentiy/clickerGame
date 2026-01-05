using System.Collections.Generic;

namespace Storage.Snapshots.LevelParams
{
    public class LevelParamsSnapshot
    {
        public LevelParamsSnapshot(int levelNumber, float figureScale, LevelBeatingTimeInfoSnapshot levelBeatingTimeInfo, List<LevelFigureParamsSnapshot> levelFiguresParamsList)
        {
            LevelNumber = levelNumber;
            LevelBeatingTimeInfo = levelBeatingTimeInfo;
            LevelFiguresParamsList = levelFiguresParamsList;
            FigureScale = figureScale;
        }

        public int LevelNumber { get; }
        public float FigureScale { get; }
        public LevelBeatingTimeInfoSnapshot LevelBeatingTimeInfo { get; }
        public List<LevelFigureParamsSnapshot> LevelFiguresParamsList { get; }
    }
}