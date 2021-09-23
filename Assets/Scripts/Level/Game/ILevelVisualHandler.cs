using System.Collections.Generic;
using Figures.Animals;
using Storage.Levels.Params;
using UnityEngine;

namespace Level.Game
{
    public interface ILevelVisualHandler
    {
        void SetupLevel(List<LevelFigureParams> levelFiguresParams, Color defaultColor);
        Camera TextureCamera { get; }
        void SetInteractivity(bool isInteractable);
    }
}