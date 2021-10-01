using System;
using Screen;
using Storage.Levels.Params;
using UnityEngine;

namespace Handlers
{
    public interface IScreenHandler
    {
        float AwaitChangeScreenTime { get; }
        void ShowChooseLevelScreen(bool fast = false);
        void ShowChoosePackScreen(bool fast = false);
        void ShowWelcomeScreen(bool fast = false);
        void StartNewLevel(int levelNumber, LevelParams levelParams, bool fast = false);
        void ReplayCurrentLevel(int levelNumber, bool fast = false);
        void PopupAllScreenHandlers();
        void ShowLevelCompleteScreen(bool onLevelEnter, Action onFinishAction, Sprite figureSprite, Gradient colorGradient, bool fast = false);
    }
}