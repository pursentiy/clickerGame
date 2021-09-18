using Level.Hud;
using UnityEngine;

namespace Handlers
{
    public interface ILevelParamsHandler
    { 
        LevelHudHandler LevelHudHandlerPrefab  { get; }
        Color TargetFigureDefaultColor { get; }
    }
}