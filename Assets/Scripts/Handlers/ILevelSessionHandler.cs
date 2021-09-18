using Level.Hud;
using Storage.Levels.Params;
using UnityEngine;

namespace Handlers
{
    public interface ILevelSessionHandler
    {
        void StartLevel(LevelParams levelParams, LevelHudHandler levelHudHandler, Color defaultColor);
    }
}