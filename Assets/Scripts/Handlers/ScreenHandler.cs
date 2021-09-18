using System;
using Installers;
using Screen;
using UnityEngine;

namespace Handlers
{
    public class ScreenHandler : InjectableMonoBehaviour, IScreenHandler
    {
        [SerializeField] private RectTransform _screenCanvasTransform;
        [SerializeField] private ChooseLevelScreenBase chooseLevelScreenBase;
        [SerializeField] private WelcomeScreenBase welcomeScreenBase;
        [SerializeField] private LevelCompleteScreenBase levelCompleteScreenBase;
        
        private Screen.ScreenBase _currentScreenBase;

        public void ShowChooseLevelScreen()
        {
            PopupAllScreenHandlers();

            _currentScreenBase = Instantiate(chooseLevelScreenBase, _screenCanvasTransform);
        }
        
        public void ShowWelcomeScreen()
        {
            PopupAllScreenHandlers();

            _currentScreenBase = Instantiate(welcomeScreenBase, _screenCanvasTransform);
        }
        
        public void ShowLevelCompleteScreen(int currentLevel, Camera sourceCamera, Action onFinishAction)
        {
            PopupAllScreenHandlers();

            var screenHandler = Instantiate(levelCompleteScreenBase, _screenCanvasTransform);
            screenHandler.SetOnFinishLevelSessionAction(onFinishAction);
            screenHandler.SetupTextureCamera(sourceCamera);
            
            _currentScreenBase = screenHandler;
            _currentScreenBase.CurrentLevel = currentLevel;
        }

        public void PopupAllScreenHandlers()
        {
            if (_currentScreenBase == null)
            {
                return;
            }
            
            Destroy(_currentScreenBase.gameObject);
            _currentScreenBase = null;
        }
    }
}