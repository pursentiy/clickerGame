using System.Collections.Generic;
using Configurations;
using Configurations.Progress;
using UnityEngine;

namespace Common.Data.Info
{
    public class LevelInfo
    {
        public int LevelId;
        public string LevelName;
        public LevelDifficulty LevelDifficulty;
        public Sprite LevelImage;
        public float FigureScale = 1f;
        public List<FigureInfo> LevelsFiguresInfo;
        public float[] BeatingTime;

        public LevelInfo(int levelId, string levelName, LevelDifficulty levelDifficulty, Sprite levelImage, float figureScale, List<FigureInfo> levelsFiguresInfo, float[] beatingTime)
        {
            LevelId = levelId;
            LevelName = levelName;
            LevelDifficulty = levelDifficulty;
            LevelImage = levelImage;
            FigureScale = figureScale;
            LevelsFiguresInfo = levelsFiguresInfo;
            BeatingTime = beatingTime;
        }
    }
}