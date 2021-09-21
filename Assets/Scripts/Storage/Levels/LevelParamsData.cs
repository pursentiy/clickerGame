using System;
using System.Collections.Generic;
using Level.Game;
using UnityEngine;

namespace Storage.Levels
{
    [Serializable]
    public class LevelParamsData
    {
        public int LevelNumber;
        public string LevelName;
        public int LevelDifficulty;
        public LevelVisualHandler LevelVisualHandler;
        public Sprite LevelImage;
        public List<LevelFigureParamsData> LevelsFiguresParams;
    }
}