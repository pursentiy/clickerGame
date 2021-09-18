using System;
using Screen;
using UnityEngine;

namespace Handlers
{
    public interface IScreenHandler
    {
        void ShowChooseLevelScreen();
        void ShowWelcomeScreen();
        void PopupAllScreenHandlers();
        void ShowLevelCompleteScreen(int currentLevel, Camera sourceCamera, Action onFinishAction);
    }
}