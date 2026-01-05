using System;
using System.Collections.Generic;

namespace Storage.Levels.Params
{
    [Serializable]
    public class LevelParams
    {
        public int LevelNumber;
        public LevelBeatingTimeInfo LevelBeatingTimeInfo;
        public List<LevelFigureParams> LevelFiguresParamsList;
    }
}