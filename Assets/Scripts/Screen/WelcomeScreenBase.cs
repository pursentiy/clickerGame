using Handlers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class WelcomeScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        
        [SerializeField] private Button _playButton;

        private void Start()
        {
            _playButton.onClick.AddListener(PushNextScreen);
        }

        private void PushNextScreen()
        {
            _screenHandler.ShowChooseLevelScreen();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
        }
    }
}