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
        public LevelDifficulty LevelDifficulty;
        public LevelVisualHandler levelVisualHandler;
        public Sprite LevelImage;
        public List<LevelFigureParamsData> LevelsFiguresParams;
    }

    [Serializable]
    public enum LevelDifficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
}