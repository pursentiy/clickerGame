using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
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
        [Inject] private UIManager _uiManager;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private RectTransform _headerText;
        [SerializeField] private CanvasGroup _headerTextCanvasGroup;
        
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
            
            AnimateShow();
        }

        private void OnBeginShow()
        {
            
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