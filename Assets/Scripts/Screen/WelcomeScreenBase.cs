using Handlers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class WelcomeScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PopupHandler _popupHandler;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;

        private void Start()
        {
            _playButton.onClick.AddListener(PushNextScreen);
            _settingsButton.onClick.AddListener(_popupHandler.ShowSettings);
        }

        private void PushNextScreen()
        {
            _screenHandler.ShowChoosePackScreen();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
        }
    }
}