using Attributes;
using Controllers;
using DG.Tweening;
using Extensions;
using Handlers.UISystem.Screens;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.WelcomeScreen
{
    [AssetKey("UI Screens/WelcomeScreenScreenMediator")]
    public sealed class WelcomeScreenMediator : UIScreenBase<WelcomeScreenView>
    {
        [Inject] private readonly FlowScreenController _flowScreenController;
        [Inject] private readonly FlowPopupController _flowPopupController;
        
        public override void OnCreated()
        {
            base.OnCreated();

            View.PlayButton.onClick.MapListenerWithSound(PushNextScreen).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
        }

        public override void OnBeginShow()
        {
            base.OnBeginShow();

            PrepareHeaderForAnimation();
            AnimateHeader();
        }

        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void PushNextScreen()
        {
            _flowScreenController.GoToChoosePackScreen();
        }
        
        private void PrepareHeaderForAnimation()
        {
            View.HeaderText.transform.localScale = Vector3.one * View.StartScale;
            View.HeaderText.transform.localPosition += new Vector3(0, -View.FlyOffset, 0);
            if (View.HeaderTextCanvasGroup != null) 
                View.HeaderTextCanvasGroup.alpha = 0;
        }
        
        private void AnimateHeader()
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