using Extensions;
using Handlers;
using Handlers.UISystem;
using Services;
using Services.CoroutineServices;
using UI.Popups.SettingsPopup;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.WelcomeScreen
{
    public class WelcomeScreen : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly CoroutineService _coroutineService;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button RESET;

        protected override void Start()
        {
            base.Start();
            
            _playButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            RESET.onClick.MapListenerWithSound(Reset).DisposeWith(this);
        }

        private void Reset()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            _coroutineService.WaitFor(0.5f).Then(() => Application.Quit()).CancelWith(this);
        }

        private void OnSettingsButtonClicked()
        {
            var context = new SettingsPopupContext(true);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }

        private void PushNextScreen()
        {
            _screenHandler.ShowChoosePackScreen();
        }
    }
}