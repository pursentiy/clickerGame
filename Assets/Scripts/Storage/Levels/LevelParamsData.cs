using System;
using System.Collections.Generic;
using Level.Game;

namespace Storage.Levels
{
    [Serializable]
    public class LevelParamsData
    {
        public int LevelNumber;
        public LevelVisualHandler LevelVisualHandler;
        public List<LevelFigureParamsData> LevelsFiguresParams;
    }
}