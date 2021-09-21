using System;
using System.Collections.Generic;
using Level.Game;
using UnityEngine;

namespace Storage.Levels.Params
{
    [Serializable]
    public class LevelParams
    {
        public int LevelNumber;
        public string LevelName;
        public Sprite PackImage;
        public int LevelDifficulty;
        public bool LevelCompleted;
        public bool LevelPlayable;
        public LevelVisualHandler LevelVisualHandler;
        public List<LevelFigureParams> LevelFiguresParamsList;
    }
}