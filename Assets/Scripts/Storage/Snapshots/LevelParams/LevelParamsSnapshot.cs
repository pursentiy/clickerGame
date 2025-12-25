using System.Collections.Generic;

namespace Storage.Snapshots.LevelParams
{
    public class LevelParamsSnapshot
    {
        public LevelParamsSnapshot(int levelNumber, LevelBeatingTimeInfoSnapshot levelBeatingTimeInfo, List<LevelFigureParamsSnapshot> levelFiguresParamsList)
        {
            LevelNumber = levelNumber;
            LevelBeatingTimeInfo = levelBeatingTimeInfo;
            LevelFiguresParamsList = levelFiguresParamsList;
        }

        public int LevelNumber { get; }
        public LevelBeatingTimeInfoSnapshot LevelBeatingTimeInfo { get; }
        public List<LevelFigureParamsSnapshot> LevelFiguresParamsList { get; }
    }
}