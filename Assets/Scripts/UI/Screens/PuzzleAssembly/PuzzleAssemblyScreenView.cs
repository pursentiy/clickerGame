using Handlers.UISystem;
using UI.Screens.PuzzleAssembly.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.PuzzleAssembly
{
    public class PuzzleAssemblyScreenView : MonoBehaviour, IUIView
    {
        public Button GoBackButton;
        public Button SettingsButton;
        public LevelTimerWidget LevelTimerWidget;
        public StarsProgressWidget StarsProgressWidget;
        public LevelSessionHandler LevelSessionHandler;
    }
}