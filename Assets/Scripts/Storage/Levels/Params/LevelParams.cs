using System;
using System.Collections.Generic;

namespace Storage.Levels.Params
{
    [Serializable]
    public class LevelParams
    {
        public int LevelNumber;
        public bool LevelCompleted;
        public bool LevelPlayable;
        public List<LevelFigureParams> LevelFiguresParamsList;
    }
}