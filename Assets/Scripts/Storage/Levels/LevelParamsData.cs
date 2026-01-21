using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage.Levels
{
    [Serializable]
    public class LevelParamsData
    {
        public int LevelId;
        public Sprite LevelImage;
        public List<LevelFigureParamsData> LevelsFiguresParams;
    }
}