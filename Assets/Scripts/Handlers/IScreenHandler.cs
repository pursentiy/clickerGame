using System;
using Screen;
using UnityEngine;

namespace Handlers
{
    public interface IScreenHandler
    {
        void ShowChooseLevelScreen(bool fast = false);
        void ShowWelcomeScreen(bool fast = false);
        void PopupAllScreenHandlers();
        void ShowLevelCompleteScreen(int currentLevel, Camera sourceCamera, Action onFinishAction, bool fast = false);
    }
}