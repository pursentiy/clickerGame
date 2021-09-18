using System;
using System.Collections.Generic;
using Level.Game;

namespace Storage.Levels.Params
{
    [Serializable]
    public class LevelParams
    {
        public int LevelNumber;
        public bool LevelCompleted;
        public bool LevelPlayable;
        public LevelVisualHandler LevelVisualHandler;
        public List<LevelFigureParams> LevelFiguresParamsList;
    }
}