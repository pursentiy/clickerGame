using System;
using Configurations;
using Configurations.Progress;
using UnityEngine;

namespace UI.Screens.ChooseLevel.LevelItem
{
    public class LevelItemWidgetInfo
    {
        public LevelItemWidgetInfo(
            string levelName,
            Sprite levelSprite,
            int totalEarnedStars,
            LevelDifficulty levelDifficulty,
            bool isUnlocked,
            Action startLevel)
        {
            LevelName = levelName;
            LevelSprite = levelSprite;
            TotalEarnedStars = totalEarnedStars;
            LevelDifficulty = levelDifficulty;
            IsUnlocked = isUnlocked;
            StartLevel = startLevel;
        }

        public string LevelName { get; }
        public Sprite LevelSprite { get; }
        public int TotalEarnedStars { get; }
        public LevelDifficulty LevelDifficulty { get; }
        public bool IsUnlocked { get; }
        public Action StartLevel { get; }
    }
}