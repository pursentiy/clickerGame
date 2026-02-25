using System.Collections.Generic;
using System.Linq;
using Attributes;
using Common.Currency;
using Controllers;
using Extensions;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.DailyReward;
using Services.ScreenBlocker;
using TMPro;
using UI.Popups.CommonPopup;
using UI.Popups.MessagePopup;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Popups.DailyRewardPopup
{
    [AssetKey("UI Popups/DailyRewardPopupMediator")]
    public class DailyRewardPopupMediator : UIPopupBase<DailyRewardPopupView, DailyRewardPopupContext>
    {
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly UIScreenBlocker _uiScreenBlocker;

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnEndShow()
        {
            base.OnEndShow();
            PlayEntranceDayItemsAnimation();
        }

        private void PlayEntranceDayItemsAnimation()
        {
            var blockRef = _uiScreenBlocker.Block();

            View.DaysController.PlayEntranceAnimation()
                .Then(Unblock)
                .CancelWith(this)
                .Catch(_ => Unblock());

            void Unblock()
            {
                if (!blockRef.IsDisposed)
                    blockRef.Dispose();
            }
        }

        public override void OnBeginHide()
        {
            base.OnBeginHide();
            
            View.DaysController.PlayExitAnimation();
        }

        public override void OnCreated()
        {
            base.OnCreated();

            SetupTexts();
            SetupButtons();
            SetupWidgets();
        }

        private void SetupWidgets()
        {
            View.DaysController.Initialize(View.DayRewardItems, Context, Hide);
        }

        private void SetupTexts()
        {
            View.TitleText.text = "Daily Reward";

            View.DayText.text = $"Day {Context.DayIndex}";
        }

        private void SetupButtons()
        {
            View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.BackgroundButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.InfoButton.onClick.MapListenerWithSound(OnInfoClicked).DisposeWith(this);
        }

        private void OnInfoClicked()
        {
            var infoText = "Daily Rewards reset every day. Claim your reward each day to continue your streak! " +
                          "If you miss a day, your streak will reset to Day 1.";
            
            var context = new MessagePopupContext(
                infoText,
                View.MainTransform,
                fontSize: 80,
                spriteAsset: null,
                facing: PopupFacing.Left);
            
            _flowPopupController.ShowMessagePopup(context);
        }


        public void PlayClaimReceivingAnimation()
        {
            View.DaysController.PlayClaimReceivingAnimation();
        }
    }
}