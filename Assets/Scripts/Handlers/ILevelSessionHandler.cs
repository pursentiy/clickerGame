using Level.Hud;
using Storage.Levels.Params;
using UnityEngine;

namespace Handlers
{
    public interface ILevelSessionHandler
    {
        void StartLevel(LevelParams packParams, LevelHudHandler levelHudHandler, Color defaultColor);
    }
}