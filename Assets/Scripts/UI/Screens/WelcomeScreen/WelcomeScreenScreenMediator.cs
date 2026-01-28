using Attributes;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Handlers.UISystem.Screens.Transitions;
using UI.Popups.SettingsPopup;
using UI.Screens.ChoosePack;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.WelcomeScreen
{
    [AssetKey("UI Screens/WelcomeScreenScreenMediator")]
    public sealed class WelcomeScreenScreenMediator : UIScreenBase<WelcomeScreenView>
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private UIManager _uiManager;
        
        public override void OnCreated()
        {
            base.OnCreated();

            View.PlayButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
        }
        
        private void OnSettingsButtonClicked()
        {
            var context = new SettingsPopupContext(true);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }

        private void PushNextScreen()
        {
            _uiManager.ScreensHandler.PushScreen(new InstantScreenTransition(typeof(ChoosePackScreenMediator), null));
        }
        
        public void PrepareForAnimation()
        {
            View.HeaderText.transform.localScale = Vector3.one * View.StartScale;
            View.HeaderText.transform.localPosition += new Vector3(0, -View.FlyOffset, 0);
            if (View.HeaderTextCanvasGroup != null) 
                View.HeaderTextCanvasGroup.alpha = 0;
        }
        
        private void AnimateShow()
        {
            View.HeaderText.transform.DOKill();
            View.HeaderTextCanvasGroup?.DOKill();
            
            View.HeaderTextCanvasGroup?.DOFade(1f, View.Duration * 0.5f).KillWith(this);
            View.HeaderText.transform.DOScale(1f, View.Duration)
                .SetEase(Ease.OutBack).KillWith(this);
            View.HeaderText.transform.DOLocalMoveY(View.HeaderText.transform.localPosition.y + View.FlyOffset, View.Duration)
                .SetEase(Ease.OutQuart).KillWith(this);
        }
    }
}