using Common.Widgets;
using Extensions;
using Handlers;
using Handlers.UISystem;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.WelcomeScreen
{
    public class WelcomeScreen : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PopupHandler _popupHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private ScaleWidget _scaleWidget;

        protected override void Awake()
        {
            base.Awake();
            
            _scaleWidget.ResetWidget();
        }
        
        private void Start()
        {
            _scaleWidget.Show();
            _playButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null)).DisposeWith(this);;
        }

        private void PushNextScreen()
        {
            _screenHandler.ShowChoosePackScreen();
        }

    }
}