using System;
using System.Collections.Generic;
using Level.Game;
using Storage.Levels.Params;
using UnityEngine;

namespace Storage.Levels
{
    [Serializable]
    public class LevelParamsData
    {
        public int LevelNumber;
        public string LevelName;
        public LevelDifficulty LevelDifficulty;
        public Sprite LevelImage;
        [Range(0.1f, 5f)]
        public float FigureScale = 1f;
        public List<LevelFigureParamsData> LevelsFiguresParams;
        public LevelBeatingTimeInfo LevelBeatingTimeInfo;
        public List<LevelFigureParams> LevelFiguresParamsList;
    }

    [Serializable]
    public enum LevelDifficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
    
    [Serializable]
    public class LevelBeatingTimeInfo
    {
        public float FastestTime;
        public float MediumTime;
        public float MinimumTime;
    }
}