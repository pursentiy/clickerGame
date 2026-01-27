using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Services;
using Services.CoroutineServices;
using Services.Player;
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
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private RectTransform _headerText;
        [SerializeField] private CanvasGroup _headerTextCanvasGroup;
        [SerializeField] private Button RESET;
        [SerializeField] private Button CURRENCY;
        
        [Header("Animation Settings")]
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _startScale = 0.5f;
        [SerializeField] private float _flyOffset = 50f;

        protected override void Awake()
        {
            base.Awake();

            PrepareForAnimation();
        }

        protected override void Start()
        {
            base.Start();
            
            _playButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            _settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            RESET.onClick.MapListenerWithSound(Reset).DisposeWith(this);
            CURRENCY.onClick.MapListenerWithSound(Currency).DisposeWith(this);
            
            AnimateShow();
        }

        private void Reset()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            _coroutineService.WaitFor(0.5f).Then(() => Application.Quit()).CancelWith(this);
        }
        
        private void Currency()
        {
            _playerCurrencyService.TryAddStars(10);
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
        
        public void PrepareForAnimation()
        {
            _headerText.transform.localScale = Vector3.one * _startScale;
            _headerText.transform.localPosition += new Vector3(0, -_flyOffset, 0);
            if (_headerTextCanvasGroup != null) 
                _headerTextCanvasGroup.alpha = 0;
        }
        
        private void AnimateShow()
        {
            _headerText.transform.DOKill();
            _headerTextCanvasGroup?.DOKill();
            
            _headerTextCanvasGroup?.DOFade(1f, _duration * 0.5f).KillWith(this);
            _headerText.transform.DOScale(1f, _duration)
                .SetEase(Ease.OutBack).KillWith(this);
            _headerText.transform.DOLocalMoveY(_headerText.transform.localPosition.y + _flyOffset, _duration)
                .SetEase(Ease.OutQuart).KillWith(this);
        }
    }
}